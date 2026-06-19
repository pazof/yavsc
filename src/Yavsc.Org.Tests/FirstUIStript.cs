
    /* Buggy
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

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
    public  void DoTestSeleniumWebSite()
    {
        var firefoxOptions = new ChromeOptions();
        firefoxOptions.AcceptInsecureCertificates = true;
        firefoxOptions.AddArgument("--headless=new");
        var driver = new ChromeDriver(firefoxOptions);

        driver.Navigate()
        .GoToUrl(_serverFixture.Addresses.FirstOrDefault(u => u.StartsWith("http:")));

        driver.Quit();
    }
    


    // FIXME [Fact]
    public async Task DoTestYavscSite()
    {
       

        var firefoxOptions = new FirefoxOptions
        {
            AcceptInsecureCertificates = true
        };

        var driver = new FirefoxDriver(firefoxOptions);

        var url = _serverFixture.Addresses.FirstOrDefault(u => u.StartsWith("http:"));
        Assert.NotNull(url);
        //driver.Navigate().GoToUrl(url);
driver.Navigate().GoToUrl("http://localhost:5000/Home/About");
        var title = driver.Title;

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

        var navbar = driver.FindElement(By.Id("navbar"));
        Assert.NotNull(navbar);
        *//*     
                var textBox = driver.FindElement(By.Name("my-text"));
                var submitButton = driver.FindElement(By.TagName("button"));

                textBox.SendKeys("Selenium");
                submitButton.Click();

                var message = driver.FindElement(By.Id("message"));
                var value = message.Text;
                    *//*  
        driver.Quit();
    }

}
*/
