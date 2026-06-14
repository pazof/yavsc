
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit.Abstractions;
using Yavsc.Tests;

namespace yavscTests.ServerFixtures;


[Collection("Yavsc Server")]

public class FirstScript : BaseTestContext
{
    public readonly WebServerFixture _serverFixture;
    readonly ITestOutputHelper _output;

    IWebDriver driver = new ChromeDriver();
    public FirstScript(ITestOutputHelper output, WebServerFixture fixture) : base(output, fixture)
    {
        _serverFixture = fixture;
        this._output = output;
    }


    [Fact]
    public async Task DoTestYavscSite()
    {
        var chromeOptions = new ChromeOptions
        {
            AcceptInsecureCertificates = true
        };

        var driver = new ChromeDriver(chromeOptions);
        var url = _serverFixture.Addresses.FirstOrDefault(u => u.StartsWith("https:"));
        Assert.NotNull(url);
        driver.Navigate().GoToUrl(url);

        var title = driver.Title;

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

        var navbar = driver.FindElement(By.Id("navbar"));
        Assert.NotNull(navbar);
        /*     
                var textBox = driver.FindElement(By.Name("my-text"));
                var submitButton = driver.FindElement(By.TagName("button"));

                textBox.SendKeys("Selenium");
                submitButton.Click();

                var message = driver.FindElement(By.Id("message"));
                var value = message.Text;
                    */
        driver.Quit();
    }


}
