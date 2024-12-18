![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/ViaData.Reqnroll.Variants)

[![](https://www.paypalobjects.com/en_GB/i/btn/btn_donate_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=LUB88PTT2RYYG)

# ViaData.Reqnroll.Variants
Reqnroll plugin to allow variants of a test to be run using tags.
For example (but not limited to) running scenarios or features against different browsers if performing UI tests.

Supports NUnit

## 1. Usage

### 1.1 Installation

Install plugin using Nuget Package Manager

```powershell
PM> Install-Package ViaData.Reqnroll.Variants
```

### 1.2 Overview
Feature variant tags mean each scenario within that feature is run for each variant.
\
i.e 4 test cases for the below two scenarios:
```gherkin
@Browser:Chrome
@Browser:Firefox
Feature: AnExampleFeature

Scenario: Simple scenario
	Given something has happened
	When I do something
	Then the result should be something else

Scenario: Simple scenario two
	Given something has happened
	When I do something
	Then the result should be something else
```
\
Scenario variant tags mean the scenario is run for each of its variants.
\
i.e 3 test cases for the below scenario:
```gherkin
Feature: AnExampleFeature

@Browser:Chrome
@Browser:Firefox
@Browser:Edge
Scenario: Simple scenario
	Given something has happened
	When I do something
	Then the result should be something
```

### 1.3 Access the variant
The variant key/value can then be accessed via the ScenarioContext static or injected class. This decision was made to cater for all initially supported test frameworks.

```csharp
[Binding]
public sealed class Hooks
{
    private readonly ScenarioContext _scenarioContext;
    private IWebDriver _driver;

    public Hooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _scenarioContext.TryGetValue("Browser", out var browser);

        switch (browser)
        {
            case "Chrome":
                _driver = SetupChromeDriver();
                break;
            case "Firefox":
                _driver = SetupFirefoxDriver();
                break;
            default:
                _driver = SetupChromeDriver();
                break;
        }
        _scenarioContext.ScenarioContainer.RegisterInstanceAs(_driver);
    }
    ...
}
```

It's also possible to use the in built contexts per test framework if desired (doesn't apply to xUnit, which is why ScenarioContext is recommended):

__NUnit__
```csharp
var categories = TestContext.CurrentContext.Test.Properties["Category"];
var browser = categories.First(a => a.ToString().StartsWith("Browser").ToString().Split(':')[1];
```

See the integration test projects for full example.

## 2. Configuration

### 2.1 Reqnroll v2+
__reqnroll.json__
\
The default variant key is 'Variant' if nothing specific is set. This means the tag `@Variant:Chrome` will be treated as a variant, where 'Chrome' is the variant value. However, the variant key can be customised in the specflow.json file:

```json
{
  "pluginparameters": {
    "variantkey": "Browser"
  }
}
```

The above means that only tags that begin with `@Browser:` will be treated as variants.

__app.config__
\
If using app.config (applicable only for .net framework), the custom variant key can be set in the following generator element and path attribute:

```XML
<configSections>
  <section name="reqnroll" type="Reqnroll.Configuration.ConfigurationSectionHandler, Reqnroll" />
</configSections>
<reqnroll>
  <generator path="VariantKey:Browser" />
</reqnroll>
```
This isn't the ideal element to use but was the best possibility we had, the path value is only treated as a variant if it starts with 'VariantKey:' meaning the generator element can be still be used as originally intended.

## License
This project uses the [MIT](https://choosealicense.com/licenses/mit/) license.
