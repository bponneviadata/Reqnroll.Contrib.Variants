﻿using OpenQA.Selenium;

namespace Reqnroll.Contrib.Variants.IntegrationTests.SharedBindings.Pages
{
    public class CommonsPage
    {
        private readonly IWebDriver _driver;

        public string Url => _driver.Url;

        public CommonsPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void NavigateSidebar(string link)
        {
            _driver.FindElement(By.LinkText(link)).Click();
        }

        public void Navigate()
        {
            _driver.Navigate().GoToUrl("http://the-internet.herokuapp.com/");
        }
    }
}
