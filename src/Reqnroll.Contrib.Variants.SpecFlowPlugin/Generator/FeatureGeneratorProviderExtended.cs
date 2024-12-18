using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;

namespace Reqnroll.Contrib.Variants.SpecFlowPlugin.Generator
{
    internal class FeatureGeneratorProviderExtended : IFeatureGeneratorProvider
    {
        private readonly IFeatureGenerator _unitTestFeatureGenerator;

        public int Priority => int.MaxValue;

        public FeatureGeneratorProviderExtended(IFeatureGenerator unitTestFeatureGenerator)
        {
            _unitTestFeatureGenerator = unitTestFeatureGenerator;
        }

        public bool CanGenerate(ReqnrollDocument document)
        {
            return true;
        }

        public IFeatureGenerator CreateGenerator(ReqnrollDocument document)
        {
            return _unitTestFeatureGenerator;
        }
    }
}