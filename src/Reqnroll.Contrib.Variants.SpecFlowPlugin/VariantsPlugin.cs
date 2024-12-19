using Reqnroll.Contrib.Variants.ReqnrollPlugin;
using Reqnroll.Contrib.Variants.ReqnrollPlugin.Generator;
using Reqnroll.Contrib.Variants.ReqnrollPlugin.Providers;
using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.Plugins;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(VariantsPlugin))]

namespace Reqnroll.Contrib.Variants.ReqnrollPlugin
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

            // Resolve reqnroll configuration to confirm custom variant key, use default if none provided
            var reqnrollConfiguration = objectContainer.Resolve<ReqnrollConfiguration>();
            var configParam = "Tenant";//reqnrollConfiguration.Plugins.FirstOrDefault(a => a.Name == GetType().Namespace.Replace(".ReqnrollPlugin", string.Empty))?.Parameters;
            _variantKey = !string.IsNullOrEmpty(configParam) ? configParam : _variantKey;

            // Create custom unit test provider based on user defined config value
            var generatorProvider = new NUnitProviderExtended(codeDomHelper, _variantKey);

            // Create generator instance to be registered and replace original
            var customFeatureGenerator = new FeatureGeneratorExtended(generatorProvider, codeDomHelper, reqnrollConfiguration, decoratorRegistry, _variantKey);
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