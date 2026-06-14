using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using Xunit.Abstractions;
using yavscTests;
using yavscTests.ServerFixtures;

namespace SeleniumDocs.GettingStarted;


    [Collection("Yavsc Server")]
public  class FirstScript : BaseTestContext
{
        public readonly WebServerFixture _serverFixture;
        readonly ITestOutputHelper _output;

        public FirstScript(ITestOutputHelper output, WebServerFixture fixture) : base(output, fixture)
        {
            _serverFixture = fixture;
            this._output = output;
        }


    [Fact]
    public  void DoTestSeleniumWebSite()
    {
        var firefoxOptions = new FirefoxOptions();
        firefoxOptions.AcceptInsecureCertificates = true;

        var driver = new FirefoxDriver(firefoxOptions);
        driver.Navigate().GoToUrl(_serverFixture.Addresses.FirstOrDefault(u => u.StartsWith("https:")));

        var title = driver.Title;

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

        var textBox = driver.FindElement(By.Name("my-text"));
        var submitButton = driver.FindElement(By.TagName("button"));
            
        textBox.SendKeys("Selenium");
        submitButton.Click();
            
        var message = driver.FindElement(By.Id("message"));
        var value = message.Text;
            
        driver.Quit();
    }
}
