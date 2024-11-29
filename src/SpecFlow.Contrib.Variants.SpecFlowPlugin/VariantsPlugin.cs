using SpecFlow.Contrib.Variants.SpecFlowPlugin;
using SpecFlow.Contrib.Variants.SpecFlowPlugin.Generator;
using SpecFlow.Contrib.Variants.SpecFlowPlugin.Providers;
using System.Linq;
using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.Plugins;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Infrastructure;
using Reqnroll.Utils;
using Reqnroll.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(VariantsPlugin))]

namespace SpecFlow.Contrib.Variants.SpecFlowPlugin
{
    public class VariantsPlugin : IGeneratorPlugin
    {
        private string _variantKey = "Variant";

        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters)
        {
            generatorPluginEvents.CustomizeDependencies += CustomizeDependencies;
        }

        public void CustomizeDependencies(object sender, CustomizeDependenciesEventArgs eventArgs)
        {
            // Resolve relevant instances to be used for custom IFeatureGenerator implementation
            var objectContainer = eventArgs.ObjectContainer;
            var language = objectContainer.Resolve<ProjectSettings>().ProjectPlatformSettings.Language;
            var codeDomHelper = objectContainer.Resolve<CodeDomHelper>(language);
            var decoratorRegistry = objectContainer.Resolve<DecoratorRegistry>();

            // Resolve specflow configuration to confirm custom variant key, use default if none provided
            var specflowConfiguration = objectContainer.Resolve<ReqnrollConfiguration>();
            var configParam = "Tenant";//specflowConfiguration.Plugins.FirstOrDefault(a => a.Name == GetType().Namespace.Replace(".SpecFlowPlugin", string.Empty))?.Parameters;
            _variantKey = !string.IsNullOrEmpty(configParam) ? configParam : _variantKey;

            // Create custom unit test provider based on user defined config value
            var generatorProvider = new NUnitProviderExtended(codeDomHelper, _variantKey);

            // Create generator instance to be registered and replace original
            var customFeatureGenerator = new FeatureGeneratorExtended(generatorProvider, codeDomHelper, specflowConfiguration, decoratorRegistry, _variantKey);
            var customFeatureGeneratorProvider = new FeatureGeneratorProviderExtended(customFeatureGenerator);

            // Register dependencies
            objectContainer.RegisterInstanceAs(generatorProvider);
            objectContainer.RegisterInstanceAs(customFeatureGenerator);
            objectContainer.RegisterInstanceAs<IFeatureGeneratorProvider>(customFeatureGeneratorProvider, "default");
        }

        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            throw new System.NotImplementedException();
        }
    }
}