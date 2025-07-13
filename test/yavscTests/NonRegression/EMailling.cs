using isnd.tests;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Abstract.Manage;

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

            AssertAsync.CompletesIn(2, () =>
                {
                    output.WriteLine("SendEMailSynchrone ...");
                    _serverFixture.MailSender.SendEmailAsync
                  (_serverFixture.SiteSettings.Owner.EMail, $"monthly email", "test boby monthly email").Wait();

                });
        }               
    }
}
