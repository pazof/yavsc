using isnd.tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Yavsc.Interface;

namespace yavscTests
{

    [Collection("EMaillingTeststCollection")]
    [Trait("regression", "II")]
    public class EMaillingTests : IClassFixture<WebServerFixture>

    {
        readonly WebServerFixture _serverFixture;
        readonly ITestOutputHelper output;
        readonly ILogger _logger;
        public EMaillingTests(WebServerFixture serverFixture, ITestOutputHelper output)
        {
            this.output = output;
            _serverFixture = serverFixture;
            _logger = serverFixture.Logger;
        }

        [Fact]
        public void SendEMailSynchrone()
        {

            using IServiceScope scope = _serverFixture.Services.CreateScope();
            ITrueEmailSender mailSender = scope.ServiceProvider.GetRequiredService<ITrueEmailSender>();

            output.WriteLine("SendEMailSynchrone ...");
            mailSender.SendEmailAsync
          (
            _serverFixture.SiteSettings.Owner.Name,
            _serverFixture.SiteSettings.Owner.EMail,
            $"monthly email",
            "test boby monthly email").Wait();

        }
    }
}
