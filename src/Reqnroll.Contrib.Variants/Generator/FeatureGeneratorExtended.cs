using Gherkin.Ast;
using Reqnroll.Contrib.Variants.Generator;
using Reqnroll.Contrib.Variants.Generator.ClassGenerator;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;
using Reqnroll.Tracing;

[assembly: InternalsVisibleTo("Reqnroll.Contrib.Variants.UnitTests")]
namespace Reqnroll.Contrib.Variants.Generator
{
    internal class FeatureGeneratorExtended : TestClassGenerator, IFeatureGenerator
    {
        private readonly IUnitTestGeneratorProvider _testGeneratorProvider;
        private readonly CodeDomHelper _codeDomHelper;
        private readonly ReqnrollConfiguration _reqnrollConfiguration;
        private readonly IDecoratorRegistry _decoratorRegistry;
        private int _tableCounter;

        private readonly VariantHelper _variantHelper;
        private List<Tag> _featureVariantTags;
        private bool _setVariantToContextForOutlineTest;
        private bool _setVariantToContextForTest;
        private string _variantValue;
        public const string CustomGeneratedComment = "Generation customised by ViaData.Reqnroll.Variants";

        public FeatureGeneratorExtended(IUnitTestGeneratorProvider testGeneratorProvider, CodeDomHelper codeDomHelper, ReqnrollConfiguration reqnrollConfiguration, IDecoratorRegistry decoratorRegistry, string variantKey)
            : base(decoratorRegistry, testGeneratorProvider, codeDomHelper, reqnrollConfiguration)
        {
            _testGeneratorProvider = testGeneratorProvider;
            _codeDomHelper = codeDomHelper;
            _reqnrollConfiguration = reqnrollConfiguration;
            _decoratorRegistry = decoratorRegistry;
            _variantHelper = new VariantHelper(variantKey);
        }

        public CodeNamespace GenerateUnitTestFixture(ReqnrollDocument document, string testClassName, string targetNamespace)
        {
            var reqnrollFeature = document.ReqnrollFeature;
            testClassName = testClassName ?? $"{reqnrollFeature.Name.ToIdentifier()}Feature";
            CreateNamespace(targetNamespace);
            CreateTestClassStructure(testClassName, document);

            SetupTestClass();
            SetupTestClassInitializeMethod();
            SetupTestClassCleanupMethod();
            SetupTestInitializeMethod();
            SetupTestCleanupMethod();

            SetupScenarioInitializeMethod(GenerationContext);
            SetupScenarioStartMethod(GenerationContext);
            SetupFeatureBackground(GenerationContext);
            SetupScenarioCleanupMethod(GenerationContext);

            var variantTags = _variantHelper.GetFeatureVariantTagValues(reqnrollFeature);
            _featureVariantTags = _variantHelper.FeatureTags(reqnrollFeature);

            if (_variantHelper.AnyScenarioHasVariantTag(reqnrollFeature) && _variantHelper.FeatureHasVariantTags)
                throw new TestGeneratorException("Variant tags were detected at feature and scenario level, please specify at one level or the other.");

            foreach (var scenarioDefinition in reqnrollFeature.ScenarioDefinitions)
            {
                if (string.IsNullOrEmpty(scenarioDefinition.Name))
                    throw new TestGeneratorException("The scenario must have a title specified.");

                if (scenarioDefinition is ScenarioOutline scenarioOutline)
                {
                    variantTags = _variantHelper.FeatureHasVariantTags ? variantTags : _variantHelper.GetScenarioVariantTagValues(scenarioDefinition);
                    GenerateScenarioOutlineTest(GenerationContext, scenarioOutline, variantTags);
                }
                else
                {
                    variantTags = _variantHelper.FeatureHasVariantTags ? variantTags : _variantHelper.GetScenarioVariantTagValues(scenarioDefinition);
                    if (variantTags.Count > 0) { variantTags.ForEach(a => GenerateTest(GenerationContext, (Scenario)scenarioDefinition, a)); }
                    else { GenerateTest(GenerationContext, (Scenario)scenarioDefinition, null); }
                }
            }
            _testGeneratorProvider.FinalizeTestClass(GenerationContext);

            CodeNamespace.Comments.Add(new CodeCommentStatement(new CodeComment(CustomGeneratedComment)));
            return CodeNamespace;
        }

        private void SetupScenarioCleanupMethod(TestClassGenerationContext generationContext)
        {
            var scenarioCleanupMethod = generationContext.ScenarioCleanupMethod;
            scenarioCleanupMethod.Attributes = MemberAttributes.Public;
            scenarioCleanupMethod.Name = "ScenarioCleanupAsync";
            var runnerExpression = GetTestRunnerExpression();
            scenarioCleanupMethod.Statements.Add(new CodeMethodInvokeExpression(runnerExpression, "CollectScenarioErrorsAsync", new CodeExpression[0]));
        }

        private void SetupScenarioStartMethod(TestClassGenerationContext generationContext)
        {
            var scenarioStartMethod = generationContext.ScenarioStartMethod;
            scenarioStartMethod.Attributes = MemberAttributes.Public;
            scenarioStartMethod.Name = "ScenarioStartAsync";
            var runnerExpression = GetTestRunnerExpression();
            scenarioStartMethod.Statements.Add(new CodeMethodInvokeExpression(runnerExpression, "OnScenarioStartAsync", new CodeExpression[0]));
        }

        private void SetupFeatureBackground(TestClassGenerationContext generationContext)
        {
            if (!generationContext.Feature.HasFeatureBackground())
                return;
            var backgroundMethod = generationContext.FeatureBackgroundMethod;
            backgroundMethod.Attributes = MemberAttributes.Public;
            backgroundMethod.Name = "FeatureBackgroundAsync";
            var background = generationContext.Feature.Background;
            _codeDomHelper.AddLineDirective(background, backgroundMethod.Statements, _reqnrollConfiguration);
            foreach (var step in background.Steps)
                GenerateStep(backgroundMethod, step, null);
            _codeDomHelper.AddLineDirectiveHidden(backgroundMethod.Statements, _reqnrollConfiguration);
        }

        private void SetupScenarioInitializeMethod(TestClassGenerationContext generationContext)
        {
            var initializeMethod = generationContext.ScenarioInitializeMethod;
            initializeMethod.Attributes = MemberAttributes.Public;
            initializeMethod.Name = "ScenarioInitialize";
            initializeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(ScenarioInfo), "scenarioInfo"));
            var runnerExpression = GetTestRunnerExpression();
            initializeMethod.Statements.Add(new CodeMethodInvokeExpression(runnerExpression, "OnScenarioInitialize", new CodeExpression[1]
            {
                new CodeVariableReferenceExpression("scenarioInfo")
            }));
        }

        private void GenerateScenarioOutlineTest(TestClassGenerationContext generationContext, ScenarioOutline scenarioOutline, List<string> variantTags = null)
        {
            scenarioOutline.ValidateExampleSetConsistency();
            var identifierMapping = scenarioOutline.CreateParamToIdentifierMapping();
            var outlineTestMethod = CreateScenarioOutlineTestMethod(generationContext, scenarioOutline, identifierMapping);

            if (generationContext.GenerateRowTests)
            {
                if (variantTags?.Count > 0)
                    GenerateScenarioOutlineExamplesAsRowTests(generationContext, scenarioOutline, outlineTestMethod, variantTags);
                else
                    GenerateScenarioOutlineExamplesAsRowTests(generationContext, scenarioOutline, outlineTestMethod, null);
            }
            else
            {
                if (variantTags?.Count > 0)
                    variantTags.ForEach(a => GenerateScenarioOutlineExamplesAsIndividualMethods(scenarioOutline, generationContext, outlineTestMethod, identifierMapping, a));
                else
                    GenerateScenarioOutlineExamplesAsIndividualMethods(scenarioOutline, generationContext, outlineTestMethod, identifierMapping, null);
            }

            var referenceExpression = new CodeVariableReferenceExpression("exampleTags");
            GenerateTestBody(generationContext, scenarioOutline, outlineTestMethod, referenceExpression, identifierMapping);
        }

        private void GenerateScenarioOutlineExamplesAsIndividualMethods(ScenarioOutline scenarioOutline, TestClassGenerationContext generationContext, CodeMemberMethod scenatioOutlineTestMethod, ParameterSubstitution paramToIdentifier, string tag = null)
        {
            int num = 0;
            foreach (var example in scenarioOutline.Examples)
            {
                var flag = example.TableBody.CanUseFirstColumnAsName();
                string str;
                if (!string.IsNullOrEmpty(example.Name))
                {
                    str = example.Name.ToIdentifier();
                }
                else
                {
                    var examples = scenarioOutline.Examples;
                    bool func(Examples es) => string.IsNullOrEmpty(es.Name);
                    str = examples.Count(func) > 1 ? $"ExampleSet {num}".ToIdentifier() : null;
                }

                foreach (var data in example.TableBody.Select((r, i) => new
                {
                    Row = r,
                    Index = i
                }))
                {
                    var variantName = flag ? data.Row.Cells.First().Value : $"Variant {data.Index}";
                    GenerateScenarioOutlineTestVariant(generationContext, scenarioOutline, scenatioOutlineTestMethod, paramToIdentifier, example.Name ?? "", str, data.Row, example.Tags, variantName, tag);
                }
                num++;
            }
        }

        private void GenerateScenarioOutlineExamplesAsRowTests(TestClassGenerationContext generationContext, ScenarioOutline scenarioOutline, CodeMemberMethod scenatioOutlineTestMethod, List<string> variantTags = null)
        {
            SetupTestMethod(generationContext, scenatioOutlineTestMethod, scenarioOutline, null, null, null, true);
            foreach (var example in scenarioOutline.Examples)
            {
                var hasVariantTags = variantTags?.Count > 0;

                if (hasVariantTags)
                {
                    scenatioOutlineTestMethod.Parameters.RemoveAt(scenatioOutlineTestMethod.Parameters.Count - 1);
                    scenatioOutlineTestMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), _variantHelper.VariantKey.ToLowerInvariant()));
                    scenatioOutlineTestMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string[]), "exampleTags"));
                    _setVariantToContextForOutlineTest = true;
                }

                foreach (var tableRow in example.TableBody)
                {
                    if (hasVariantTags)
                    {
                        foreach (var variant in variantTags)
                        {
                            var arguments = tableRow.Cells.Select(c => c.Value).ToList();
                            arguments.Add($"{_variantHelper.VariantKey}:{variant}");
                            _testGeneratorProvider.SetRow(generationContext, scenatioOutlineTestMethod, arguments, example.Tags.GetTagsExcept("@Ignore"), example.Tags.HasTag("@Ignore"));
                        }
                    }
                    else
                    {
                        var arguments = tableRow.Cells.Select(c => c.Value).ToList();
                        _testGeneratorProvider.SetRow(generationContext, scenatioOutlineTestMethod, arguments, example.Tags.GetTagsExcept("@Ignore"), example.Tags.HasTag("@Ignore"));
                    }
                }
            }
        }

        private CodeMemberMethod CreateScenarioOutlineTestMethod(TestClassGenerationContext generationContext, ScenarioOutline scenarioOutline, ParameterSubstitution paramToIdentifier)
        {
            var method = generationContext.TestClass.CreateMethod();
            method.Attributes = MemberAttributes.Public;
            method.Name = scenarioOutline.Name.ToIdentifier();
            foreach (var keyValuePair in paramToIdentifier)
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), keyValuePair.Value));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string[]), "exampleTags"));
            return method;
        }

        private void GenerateScenarioOutlineTestVariant(TestClassGenerationContext generationContext, ScenarioOutline scenarioOutline, CodeMemberMethod scenatioOutlineTestMethod, IEnumerable<KeyValuePair<string, string>> paramToIdentifier, string exampleSetTitle, string exampleSetIdentifier, Gherkin.Ast.TableRow row, IEnumerable<Tag> exampleSetTags, string variantName, string tag = null)
        {
            variantName = string.IsNullOrEmpty(tag) ? variantName : $"{variantName}_{tag}";
            var testMethod = CreateTestMethod(generationContext, scenarioOutline, exampleSetTags, variantName, exampleSetIdentifier);
            _codeDomHelper.AddLineDirective(scenarioOutline, testMethod.Statements, _reqnrollConfiguration);
            var list1 = new List<CodeExpression>();
            list1.AddRange(row.Cells.Select(paramCell => new CodePrimitiveExpression(paramCell.Value)).Cast<CodeExpression>().ToList());
            list1.Add(exampleSetTags.GetStringArrayExpression());

            if (tag != null)
            {
                var s = new CodePrimitiveExpression(tag);
                list1.Add(s);
                _setVariantToContextForOutlineTest = true;
            }

            testMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), scenatioOutlineTestMethod.Name, list1.ToArray()));
            _codeDomHelper.AddLineDirectiveHidden(testMethod.Statements, _reqnrollConfiguration);
            var list2 = paramToIdentifier.Select((p2i, paramIndex) => new KeyValuePair<string, string>(p2i.Key, row.Cells.ElementAt(paramIndex).Value)).ToList();
            _testGeneratorProvider.SetTestMethodAsRow(generationContext, testMethod, scenarioOutline.Name, exampleSetTitle, variantName, list2);
        }

        private CodeMemberMethod CreateTestMethod(TestClassGenerationContext generationContext, StepsContainer scenario, IEnumerable<Tag> additionalTags, string variantName = null, string exampleSetIdentifier = null)
        {
            var method = generationContext.TestClass.CreateMethod();
            SetupTestMethod(generationContext, method, scenario, additionalTags, variantName, exampleSetIdentifier, false);
            return method;
        }

        private void GenerateTest(TestClassGenerationContext generationContext, Scenario scenario, string tag = null)
        {
            string variantName = null;
            if (!string.IsNullOrEmpty(tag))
            {
                variantName = $"_{tag}";
                _setVariantToContextForTest = true;
                _variantValue = tag;
            }

            var testMethod = CreateTestMethod(generationContext, scenario, null, variantName, null);
            GenerateTestBody(generationContext, scenario, testMethod, null, null);
        }

        private void AddVariableForArguments(CodeMemberMethod testMethod, ParameterSubstitution paramToIdentifier)
        {
            var argumentsExpression = new CodeVariableDeclarationStatement(
                typeof(OrderedDictionary),
                GeneratorConstants.SCENARIO_ARGUMENTS_VARIABLE_NAME,
                new CodeObjectCreateExpression(typeof(OrderedDictionary)));

            testMethod.Statements.Add(argumentsExpression);

            if (paramToIdentifier != null)
            {
                foreach (var parameter in paramToIdentifier)
                {
                    var addArgumentExpression = new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression(new CodeTypeReference(GeneratorConstants.SCENARIO_ARGUMENTS_VARIABLE_NAME)),
                            nameof(OrderedDictionary.Add)),
                        new CodePrimitiveExpression(parameter.Key),
                        new CodeVariableReferenceExpression(parameter.Value));

                    testMethod.Statements.Add(addArgumentExpression);
                }
            }
        }

        private void GenerateTestBody(TestClassGenerationContext generationContext, StepsContainer scenario, CodeMemberMethod testMethod, CodeExpression additionalTagsExpression = null, ParameterSubstitution paramToIdentifier = null)
        {
            CodeExpression left;
            if (additionalTagsExpression == null)
                left = scenario.GetTags().GetStringArrayExpression();
            else if (!scenario.HasTags())
            {
                left = additionalTagsExpression;
            }
            else
            {
                testMethod.Statements.Add(new CodeVariableDeclarationStatement(typeof(string[]), "__tags", scenario.GetTags().GetStringArrayExpression()));
                left = new CodeVariableReferenceExpression("__tags");
                testMethod.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(additionalTagsExpression, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)), new CodeStatement[1]
                {
                    new CodeAssignStatement(left, new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof (Enumerable)), "ToArray", new CodeExpression[1]
                    {
                      new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof (Enumerable)), "Concat", new CodeExpression[2]
                      {
                        left,
                        additionalTagsExpression
                      })
                    }))
                }));
            }

            AddVariableForArguments(testMethod, paramToIdentifier);

            if (_setVariantToContextForOutlineTest)
            {
                var scenarioInfoExpression = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(null, "string.Format"), new CodeExpression[3]
                {
                    new CodePrimitiveExpression("{0}: {1}"),
                    new CodePrimitiveExpression(scenario.Name),
                    new CodeVariableReferenceExpression(_variantHelper.VariantKey.ToLowerInvariant())
                });

                testMethod.Statements.Add(new CodeVariableDeclarationStatement(typeof(ScenarioInfo), "scenarioInfo", new CodeObjectCreateExpression(typeof(ScenarioInfo), new CodeExpression[4]
                {
                    scenarioInfoExpression,
                    new CodePrimitiveExpression(scenario.Description),
                    left,
                    new CodeVariableReferenceExpression(GeneratorConstants.SCENARIO_ARGUMENTS_VARIABLE_NAME)
                })));
                testMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), generationContext.ScenarioInitializeMethod.Name, new CodeExpression[1]
                {
                    new CodeVariableReferenceExpression("scenarioInfo")
                }));

            }
            else if (_setVariantToContextForTest)
            {
                var scenarioInfoName = $"{scenario.Name}: {_variantValue}";
                testMethod.Statements.Add(new CodeVariableDeclarationStatement(typeof(ScenarioInfo), "scenarioInfo", new CodeObjectCreateExpression(typeof(ScenarioInfo), new CodeExpression[4]
                {
                    new CodePrimitiveExpression(scenarioInfoName),
                    new CodePrimitiveExpression(scenario.Description),
                    left,
                    new CodeVariableReferenceExpression(GeneratorConstants.SCENARIO_ARGUMENTS_VARIABLE_NAME)
                })));
                _codeDomHelper.AddLineDirective(scenario, testMethod.Statements, _reqnrollConfiguration);
                testMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), generationContext.ScenarioInitializeMethod.Name, new CodeExpression[1]
                {
                    new CodeVariableReferenceExpression("scenarioInfo")
                }));
            }
            else
            {
                testMethod.Statements.Add(new CodeVariableDeclarationStatement(typeof(ScenarioInfo), "scenarioInfo", new CodeObjectCreateExpression(typeof(ScenarioInfo), new CodeExpression[4]
                {
                    new CodePrimitiveExpression(scenario.Name),
                    new CodePrimitiveExpression(scenario.Description),
                    left,
                    new CodeVariableReferenceExpression(GeneratorConstants.SCENARIO_ARGUMENTS_VARIABLE_NAME)
                })));
                _codeDomHelper.AddLineDirective(scenario, testMethod.Statements, _reqnrollConfiguration);
                testMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), generationContext.ScenarioInitializeMethod.Name, new CodeExpression[1]
                {
                    new CodeVariableReferenceExpression("scenarioInfo")
                }));
            }
            if (_setVariantToContextForOutlineTest)
            {
                testMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(null, generationContext.TestRunnerField.Name), "ScenarioContext"), "Add", null), new CodeExpression[2]
                {
                    new CodePrimitiveExpression(_variantHelper.VariantKey),
                    new CodeVariableReferenceExpression(_variantHelper.VariantKey.ToLowerInvariant())
                }));

                if (!generationContext.GenerateRowTests)
                    testMethod.Parameters.Add(new CodeParameterDeclarationExpression("System.String", _variantHelper.VariantKey.ToLowerInvariant()));
            }
            else if (_setVariantToContextForTest)
            {
                testMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(null, generationContext.TestRunnerField.Name), "ScenarioContext"), "Add", null), new CodeExpression[2]
                {
                    new CodePrimitiveExpression(_variantHelper.VariantKey),
                    new CodePrimitiveExpression(_variantValue)
                }));
            }

            _setVariantToContextForOutlineTest = false;
            _setVariantToContextForTest = false;
            _variantValue = null;

            testMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), generationContext.ScenarioStartMethod.Name, new CodeExpression[0]));
            if (generationContext.Feature.HasFeatureBackground())
            {
                _codeDomHelper.AddLineDirective(generationContext.Feature.Background, testMethod.Statements, _reqnrollConfiguration);
                testMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), generationContext.FeatureBackgroundMethod.Name, new CodeExpression[0]));
            }
            foreach (var step in scenario.Steps)
                GenerateStep(testMethod, step, paramToIdentifier);
            _codeDomHelper.AddLineDirectiveHidden(testMethod.Statements, _reqnrollConfiguration);
            testMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), generationContext.ScenarioCleanupMethod.Name, new CodeExpression[0]));
        }

        private void SetupTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, StepsContainer scenarioDefinition, IEnumerable<Tag> additionalTags, string variantName, string exampleSetIdentifier, bool rowTest = false)
        {
            testMethod.Attributes = MemberAttributes.Public;
            testMethod.Name = GetTestMethodName(scenarioDefinition, variantName, exampleSetIdentifier);
            var str = scenarioDefinition.Name;
            if (variantName != null)
            {
                if (variantName.IndexOf("_") == 0) { variantName = variantName.Remove(0, 1); }
                str = $"{scenarioDefinition.Name}: {variantName}";
            }
            if (rowTest)
                _testGeneratorProvider.SetRowTest(generationContext, testMethod, str);
            else
                _testGeneratorProvider.SetTestMethod(generationContext, testMethod, str);
            _decoratorRegistry.DecorateTestMethod(generationContext, testMethod, scenarioDefinition.GetTags().ConcatTags(additionalTags).ConcatTags(_featureVariantTags), out List<string> unprocessedTags);

            if (!unprocessedTags.Any())
                return;

            _testGeneratorProvider.SetTestMethodCategories(generationContext, testMethod, unprocessedTags);
        }

        private void GenerateStep(CodeMemberMethod testMethod, Step gherkinStep, ParameterSubstitution paramToIdentifier)
        {
            var reqnrollStep = gherkinStep.AsReqnrollStep();
            var codeExpressionList = new List<CodeExpression> { paramToIdentifier.GetSubstitutedString(reqnrollStep.Text) };
            if (reqnrollStep.Argument != null)
                _codeDomHelper.AddLineDirectiveHidden(testMethod.Statements, _reqnrollConfiguration);
            codeExpressionList.Add(paramToIdentifier.GetSubstitutedString((reqnrollStep.Argument as DocString)?.Content));
            codeExpressionList.Add(GetTableArgExpression(reqnrollStep.Argument as Gherkin.Ast.DataTable, testMethod.Statements, paramToIdentifier));
            codeExpressionList.Add(new CodePrimitiveExpression(reqnrollStep.Keyword));
            _codeDomHelper.AddLineDirective(reqnrollStep, testMethod.Statements, _reqnrollConfiguration);
            var runnerExpression = GetTestRunnerExpression();
            testMethod.Statements.Add(new CodeMethodInvokeExpression(runnerExpression, reqnrollStep.StepKeyword.ToString() + "Async", codeExpressionList.ToArray()));
        }

        private string GetTestMethodName(StepsContainer scenario, string variantName, string exampleSetIdentifier)
        {
            var str1 = scenario.Name.ToIdentifier();
            if (variantName != null)
            {
                var str2 = variantName.ToIdentifier().TrimStart('_');
                str1 = string.IsNullOrEmpty(exampleSetIdentifier) ? $"{str1}_{str2}" : $"{str1}_{exampleSetIdentifier}_{str2}";
            }
            return str1;
        }

        private CodeExpression GetTableArgExpression(Gherkin.Ast.DataTable tableArg, CodeStatementCollection statements, ParameterSubstitution paramToIdentifier)
        {
            if (tableArg == null)
                return new CodeCastExpression(typeof(Table), new CodePrimitiveExpression(null));
            _tableCounter++;
            var tableRow1 = tableArg.Rows.First();
            var array = tableArg.Rows.Skip(1).ToArray();
            var referenceExpression = new CodeVariableReferenceExpression("table" + _tableCounter);
            statements.Add(new CodeVariableDeclarationStatement(typeof(Table), referenceExpression.VariableName, new CodeObjectCreateExpression(typeof(Table), new CodeExpression[1]
            {
                tableRow1.Cells.Select(c => c.Value).GetStringArrayExpression(paramToIdentifier)
            })));
            foreach (Gherkin.Ast.TableRow tableRow2 in array)
            {
                statements.Add(new CodeMethodInvokeExpression(referenceExpression, "AddRow", new CodeExpression[1]
                {
                    tableRow2.Cells.Select(c => c.Value).GetStringArrayExpression(paramToIdentifier)
                }));
            }
            return referenceExpression;
        }
    }
}