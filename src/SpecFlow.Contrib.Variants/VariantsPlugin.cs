using SpecFlow.Contrib.Variants;
using SpecFlow.Contrib.Variants.Generator;
using SpecFlow.Contrib.Variants.Providers;
using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.Plugins;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(VariantsPlugin))]

namespace SpecFlow.Contrib.Variants
{
    public class VariantsPlugin : IGeneratorPlugin
    {
        private string _variantKey = "Variant";
        private string utp;

        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            utp = unitTestProviderConfiguration.UnitTestProvider;
            generatorPluginEvents.CustomizeDependencies += CustomizeDependencies;
        }

        public void CustomizeDependencies(object sender, CustomizeDependenciesEventArgs eventArgs)
        {
            // Resolve relevant instances to be used for custom IFeatureGenerator implementation
            var objectContainer = eventArgs.ObjectContainer;
            var language = objectContainer.Resolve<ProjectSettings>().ProjectPlatformSettings.Language;
            var codeDomHelper = objectContainer.Resolve<CodeDomHelper>(language);
            var decoratorRegistry = objectContainer.Resolve<DecoratorRegistry>();

            // Get variant key from config
            var projSettings = eventArgs.ObjectContainer.Resolve<ProjectSettings>();
            if (projSettings.ConfigurationHolder.HasConfiguration && projSettings.ConfigurationHolder.ConfigSource == ConfigSource.Json)
            {
                var vk = GetJsonValueByRegex(projSettings.ConfigurationHolder.Content, "variantkey");
                _variantKey = !string.IsNullOrEmpty(vk) ? vk : _variantKey;
            }
            else if (projSettings.ConfigurationHolder.HasConfiguration && projSettings.ConfigurationHolder.ConfigSource == ConfigSource.AppConfig)
            {
                var appconfig = projSettings.ConfigurationHolder.Content;
                var vk = GetGeneratorPath(appconfig);
                _variantKey = !string.IsNullOrEmpty(vk) ? vk : _variantKey;
            }

            // Create custom unit test provider based on user defined config value
            if (string.IsNullOrEmpty(utp))
            {
                var c = objectContainer.Resolve<UnitTestProviderConfiguration>();
                utp = c.UnitTestProvider;

                if (string.IsNullOrEmpty(utp))
                    throw new Exception("Unit test provider not detected, please install as a nuget package described here: https://github.com/SpecFlowOSS/SpecFlow/wiki/SpecFlow-and-.NET-Core");
            }

            var generatorProvider = GetGeneratorProviderFromConfig(codeDomHelper, utp);
            var specflowConfiguration = eventArgs.ReqnrollProjectConfiguration.ReqnrollConfiguration;

            // Create generator instance to be registered and replace original
            var customFeatureGenerator = new FeatureGeneratorExtended(generatorProvider, codeDomHelper, specflowConfiguration, decoratorRegistry, _variantKey);
            var customFeatureGeneratorProvider = new FeatureGeneratorProviderExtended(customFeatureGenerator);

            // Register dependencies
            objectContainer.RegisterInstanceAs(generatorProvider);
            objectContainer.RegisterInstanceAs(customFeatureGenerator);
            objectContainer.RegisterInstanceAs<IFeatureGeneratorProvider>(customFeatureGeneratorProvider, "default");
        }

        private string GetJsonValueByRegex(string config, string key)
        {
            var reg = new Regex($@"(?<={key}\""\:\"").+?(?=\"")", RegexOptions.IgnoreCase);
            var match = reg.Match(config?.Replace(" ", "") ?? "");
            return match.Success ? match.Value : "";
        }

        private IUnitTestGeneratorProvider GetGeneratorProviderFromConfig(CodeDomHelper codeDomHelper, string config) => new NUnitProviderExtended(codeDomHelper, _variantKey);

        private string GetGeneratorPath(string config)
        {
            var browser = XElement.Parse(config);
            var el = browser.Element("generator");
            var attribute = el?.Attribute("path")?.Value ?? string.Empty;

            return attribute.StartsWith("variantkey", StringComparison.InvariantCultureIgnoreCase)
                ? Regex.Replace(attribute, "variantkey:", "", RegexOptions.IgnoreCase) : string.Empty;
        }
    }
}