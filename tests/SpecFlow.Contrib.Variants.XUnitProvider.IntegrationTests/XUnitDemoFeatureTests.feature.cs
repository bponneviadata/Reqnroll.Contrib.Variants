﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.4.0.0
//      SpecFlow Generator Version:2.4.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SpecFlow.Contrib.Variants.XUnitProvider.IntegrationTests
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "Browser:Chrome")]
    [Xunit.TraitAttribute("Category", "Browser:Firefox")]
    public partial class XUnitDemoFeatureTestsFeature : Xunit.IClassFixture<XUnitDemoFeatureTestsFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "XUnitDemoFeatureTests.feature"
#line hidden
        
        public XUnitDemoFeatureTestsFeature(XUnitDemoFeatureTestsFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "XUnitDemoFeatureTests", "\tIn order to verify the SpecFlow variants plugin for features\r\n\tAs a developer\r\n\t" +
                    "I want to be able to run integration tests to validate the plugin", ProgrammingLanguage.CSharp, new string[] {
                        "Browser:Chrome",
                        "Browser:Firefox"});
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 8
#line 9
 testRunner.Given("I am on the Google home page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="A single test without examples or tags: Chrome")]
        [Xunit.TraitAttribute("FeatureTitle", "XUnitDemoFeatureTests")]
        [Xunit.TraitAttribute("Description", "A single test without examples or tags: Chrome")]
        [Xunit.TraitAttribute("Category", "Browser:Chrome")]
        public virtual void ASingleTestWithoutExamplesOrTags_Chrome()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A single test without examples or tags", null, ((string[])(null)));
#line 11
this.ScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.Add("Browser", "Chrome");
            this.ScenarioStart();
#line 8
this.FeatureBackground();
#line 12
 testRunner.When("I search for \'totaltest github\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 13
 testRunner.Then("the following result should be listed:", "TotalTest (Prab) · GitHub", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="A single test without examples or tags: Firefox")]
        [Xunit.TraitAttribute("FeatureTitle", "XUnitDemoFeatureTests")]
        [Xunit.TraitAttribute("Description", "A single test without examples or tags: Firefox")]
        [Xunit.TraitAttribute("Category", "Browser:Firefox")]
        public virtual void ASingleTestWithoutExamplesOrTags_Firefox()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A single test without examples or tags", null, ((string[])(null)));
#line 11
this.ScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.Add("Browser", "Firefox");
            this.ScenarioStart();
#line 8
this.FeatureBackground();
#line 12
 testRunner.When("I search for \'totaltest github\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 13
 testRunner.Then("the following result should be listed:", "TotalTest (Prab) · GitHub", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="A test with non-variant tags: Chrome")]
        [Xunit.TraitAttribute("FeatureTitle", "XUnitDemoFeatureTests")]
        [Xunit.TraitAttribute("Description", "A test with non-variant tags: Chrome")]
        [Xunit.TraitAttribute("Category", "Browser:Chrome")]
        [Xunit.TraitAttribute("Category", "Settings")]
        [Xunit.TraitAttribute("Category", "Tools")]
        public virtual void ATestWithNon_VariantTags_Chrome()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A test with non-variant tags", null, new string[] {
                        "Settings",
                        "Tools"});
#line 20
this.ScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.Add("Browser", "Chrome");
            this.ScenarioStart();
#line 8
this.FeatureBackground();
#line 21
 testRunner.When("I search for \'totaltest github\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 22
 testRunner.Then("there should be links to the tags specified", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="A test with non-variant tags: Firefox")]
        [Xunit.TraitAttribute("FeatureTitle", "XUnitDemoFeatureTests")]
        [Xunit.TraitAttribute("Description", "A test with non-variant tags: Firefox")]
        [Xunit.TraitAttribute("Category", "Browser:Firefox")]
        [Xunit.TraitAttribute("Category", "Settings")]
        [Xunit.TraitAttribute("Category", "Tools")]
        public virtual void ATestWithNon_VariantTags_Firefox()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A test with non-variant tags", null, new string[] {
                        "Settings",
                        "Tools"});
#line 20
this.ScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.Add("Browser", "Firefox");
            this.ScenarioStart();
#line 8
this.FeatureBackground();
#line 21
 testRunner.When("I search for \'totaltest github\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 22
 testRunner.Then("there should be links to the tags specified", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        public virtual void ATestWithVariantTagsAndExamples(string repo, string site, string[] exampleTags, string browser)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A test with variant tags and examples", null, exampleTags);
#line 24
this.ScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.Add("Browser", browser);
            this.ScenarioStart();
#line 8
this.FeatureBackground();
#line 25
 testRunner.And("I navigate to the \'TotalTest\' Github page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 26
 testRunner.When(string.Format("I drill into the \'{0}\' repository", repo), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 27
 testRunner.Then(string.Format("I should be on the website \'{0}\'", site), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="A test with variant tags and examples: totaltest.github.io_Chrome")]
        [Xunit.TraitAttribute("FeatureTitle", "XUnitDemoFeatureTests")]
        [Xunit.TraitAttribute("Description", "A test with variant tags and examples: totaltest.github.io_Chrome")]
        [Xunit.TraitAttribute("Category", "Browser:Chrome")]
        public virtual void ATestWithVariantTagsAndExamples_Totaltest_Github_Io_Chrome()
        {
#line 24
this.ATestWithVariantTagsAndExamples("totaltest.github.io", "https://github.com/TotalTest/totaltest.github.io", ((string[])(null)), "Chrome");
#line hidden
        }
        
        [Xunit.FactAttribute(DisplayName="A test with variant tags and examples: SpecFlow.Contrib.Variants_Chrome")]
        [Xunit.TraitAttribute("FeatureTitle", "XUnitDemoFeatureTests")]
        [Xunit.TraitAttribute("Description", "A test with variant tags and examples: SpecFlow.Contrib.Variants_Chrome")]
        [Xunit.TraitAttribute("Category", "Browser:Chrome")]
        public virtual void ATestWithVariantTagsAndExamples_SpecFlow_Contrib_Variants_Chrome()
        {
#line 24
this.ATestWithVariantTagsAndExamples("SpecFlow.Contrib.Variants", "https://github.com/TotalTest/SpecFlow.Contrib.Variants", ((string[])(null)), "Chrome");
#line hidden
        }
        
        [Xunit.FactAttribute(DisplayName="A test with variant tags and examples: totaltest.github.io_Firefox")]
        [Xunit.TraitAttribute("FeatureTitle", "XUnitDemoFeatureTests")]
        [Xunit.TraitAttribute("Description", "A test with variant tags and examples: totaltest.github.io_Firefox")]
        [Xunit.TraitAttribute("Category", "Browser:Firefox")]
        public virtual void ATestWithVariantTagsAndExamples_Totaltest_Github_Io_Firefox()
        {
#line 24
this.ATestWithVariantTagsAndExamples("totaltest.github.io", "https://github.com/TotalTest/totaltest.github.io", ((string[])(null)), "Firefox");
#line hidden
        }
        
        [Xunit.FactAttribute(DisplayName="A test with variant tags and examples: SpecFlow.Contrib.Variants_Firefox")]
        [Xunit.TraitAttribute("FeatureTitle", "XUnitDemoFeatureTests")]
        [Xunit.TraitAttribute("Description", "A test with variant tags and examples: SpecFlow.Contrib.Variants_Firefox")]
        [Xunit.TraitAttribute("Category", "Browser:Firefox")]
        public virtual void ATestWithVariantTagsAndExamples_SpecFlow_Contrib_Variants_Firefox()
        {
#line 24
this.ATestWithVariantTagsAndExamples("SpecFlow.Contrib.Variants", "https://github.com/TotalTest/SpecFlow.Contrib.Variants", ((string[])(null)), "Firefox");
#line hidden
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                XUnitDemoFeatureTestsFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                XUnitDemoFeatureTestsFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion

