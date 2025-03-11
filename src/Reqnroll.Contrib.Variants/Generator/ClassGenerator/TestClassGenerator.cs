using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;

namespace Reqnroll.Contrib.Variants.Generator.ClassGenerator
{
    internal class TestClassGenerator
    {
        protected CodeNamespace CodeNamespace { get; private set; }
        protected TestClassGenerationContext GenerationContext { get; private set; }

        private readonly IDecoratorRegistry _decoratorRegistry;
        private readonly IUnitTestGeneratorProvider _testGeneratorProvider;
        private readonly CodeDomHelper _codeDomHelper;
        private readonly ReqnrollConfiguration _reqnrollConfiguration;

        public TestClassGenerator(IDecoratorRegistry decoratorRegistry, IUnitTestGeneratorProvider testGeneratorProvider, CodeDomHelper codeDomHelper, ReqnrollConfiguration reqnrollConfiguration)
        {
            _decoratorRegistry = decoratorRegistry;
            _testGeneratorProvider = testGeneratorProvider;
            _codeDomHelper = codeDomHelper;
            _reqnrollConfiguration = reqnrollConfiguration;
        }

        public void CreateNamespace(string targetNamespace)
        {
            targetNamespace = targetNamespace ?? "ReqnrollTests";
            if (!targetNamespace.StartsWith("global", StringComparison.CurrentCultureIgnoreCase) &&
                _codeDomHelper.TargetLanguage == CodeDomProviderLanguage.VB)
            {
                targetNamespace = $"GlobalVBNetNamespace.{targetNamespace}";
            }

            CodeNamespace = new CodeNamespace(targetNamespace)
            {
                Imports = { 
                    new CodeNamespaceImport("Reqnroll")
                }
            };
        }

        public void CreateTestClassStructure(string testClassName, ReqnrollDocument document)
        {
            var generatedTypeDeclaration = _codeDomHelper.CreateGeneratedTypeDeclaration(testClassName);
            CodeNamespace.Types.Add(generatedTypeDeclaration);
            GenerationContext = new TestClassGenerationContext(
                _testGeneratorProvider,
                document,
                CodeNamespace,
                generatedTypeDeclaration,
                DeclareTestRunnerMember(generatedTypeDeclaration),
                generatedTypeDeclaration.CreateMethod(),
                generatedTypeDeclaration.CreateMethod(),
                generatedTypeDeclaration.CreateMethod(),
                generatedTypeDeclaration.CreateMethod(),
                generatedTypeDeclaration.CreateMethod(),
                generatedTypeDeclaration.CreateMethod(),
                generatedTypeDeclaration.CreateMethod(),
                document.ReqnrollFeature.HasFeatureBackground() ? generatedTypeDeclaration.CreateMethod() : null,
                _testGeneratorProvider.GetTraits().HasFlag(UnitTestGeneratorTraits.RowTests) && _reqnrollConfiguration.AllowRowTests);
        }

        private CodeMemberField DeclareTestRunnerMember(CodeTypeDeclaration type)
        {
            var testRunnerField = new CodeMemberField(typeof(ITestRunner).FullName,
                GeneratorConstants.TESTRUNNER_FIELD);
            testRunnerField.Attributes = MemberAttributes.Static;
            type.Members.Add(testRunnerField);
            return testRunnerField;
        }

        public void SetupTestClass()
        {
            GenerationContext.TestClass.IsPartial = true;
            GenerationContext.TestClass.TypeAttributes |= TypeAttributes.Public;
            _codeDomHelper.AddLinePragmaInitial(GenerationContext.TestClass, GenerationContext.Document.SourceFilePath, _reqnrollConfiguration);
            _testGeneratorProvider.SetTestClass(GenerationContext, GenerationContext.Feature.Name, GenerationContext.Feature.Description);
            _decoratorRegistry.DecorateTestClass(GenerationContext, out List<string> unprocessedTags);
            if (!unprocessedTags.Any())
                return;
            _testGeneratorProvider.SetTestClassCategories(GenerationContext, unprocessedTags);
        }

        public void SetupTestClassInitializeMethod()
        {
            var initializeMethod = GenerationContext.TestClassInitializeMethod;
            initializeMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            initializeMethod.Name = "FeatureSetupAsync";
            _codeDomHelper.MarkCodeMemberMethodAsAsync(initializeMethod);

            _testGeneratorProvider.SetTestClassInitializeMethod(GenerationContext);

            CodeExpression[] codeExpressionArray1;
            if (!_testGeneratorProvider.GetTraits().HasFlag(UnitTestGeneratorTraits.ParallelExecution))
            {
                codeExpressionArray1 = (new CodePrimitiveExpression[2]
                {
                    new CodePrimitiveExpression(null),
                    new CodePrimitiveExpression(0)
                });
            }
            else
            {
                codeExpressionArray1 = new CodeExpression[0];
            }

            var codeExpressionArray2 = codeExpressionArray1;

            CodeExpression runnerExpression = GetTestRunnerExpression();

            initializeMethod.Statements.Add(new CodeAssignStatement(runnerExpression, new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(TestRunnerManager)), "GetTestRunnerForAssembly", codeExpressionArray2)));
            initializeMethod.Statements.Add(new CodeVariableDeclarationStatement(typeof(FeatureInfo), "featureInfo", new CodeObjectCreateExpression(typeof(FeatureInfo), new CodeExpression[6]
            {
                new CodeObjectCreateExpression(typeof(CultureInfo), new CodeExpression[1]
                {
                     new CodePrimitiveExpression(GenerationContext.Feature.Language)
                }),
                new CodePrimitiveExpression(GenerationContext.Document.DocumentLocation.FeatureFolderPath),
                new CodePrimitiveExpression(GenerationContext.Feature.Name),
                new CodePrimitiveExpression(GenerationContext.Feature.Description),
                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("ProgrammingLanguage"), _codeDomHelper.TargetLanguage.ToString()),
                GenerationContext.Feature.Tags.GetStringArrayExpression()
            })));

            var expression = new CodeMethodInvokeExpression(runnerExpression, "OnFeatureStartAsync", new CodeExpression[1]
            {
                new CodeVariableReferenceExpression("featureInfo")
            });
            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

            initializeMethod.Statements.Add(expression);
        }

        public void SetupTestInitializeMethod()
        {
            var initializeMethod = GenerationContext.TestInitializeMethod;
            initializeMethod.Attributes = MemberAttributes.Public;
            initializeMethod.Name = "TestInitialize";

            _testGeneratorProvider.SetTestInitializeMethod(GenerationContext);
        }

        public void SetupTestCleanupMethod()
        {
            var testCleanupMethod = GenerationContext.TestCleanupMethod;
            testCleanupMethod.Attributes = MemberAttributes.Public;
            testCleanupMethod.Name = "ScenarioTearDownAsync";

            _codeDomHelper.MarkCodeMemberMethodAsAsync(testCleanupMethod);
            _testGeneratorProvider.SetTestCleanupMethod(GenerationContext);

            var runnerExpression = GetTestRunnerExpression();

            var expression = new CodeMethodInvokeExpression(runnerExpression, "OnScenarioEndAsync", new CodeExpression[0]);
            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

            testCleanupMethod.Statements.Add(expression);
        }

        public void SetupTestClassCleanupMethod()
        {
            var classCleanupMethod = GenerationContext.TestClassCleanupMethod;
            classCleanupMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            classCleanupMethod.Name = "FeatureTearDownAsync";

            _codeDomHelper.MarkCodeMemberMethodAsAsync(classCleanupMethod);
            _testGeneratorProvider.SetTestClassCleanupMethod(GenerationContext);

            var runnerExpression = GetTestRunnerExpression();

            var expression = new CodeMethodInvokeExpression(runnerExpression, "OnFeatureEndAsync", new CodeExpression[0]);
            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

            classCleanupMethod.Statements.Add(expression);
            classCleanupMethod.Statements.Add(new CodeAssignStatement(runnerExpression, new CodePrimitiveExpression(null)));
        }

        protected CodeExpression GetTestRunnerExpression()
        {
            return new CodeVariableReferenceExpression("testRunner");
        }
    }
}
