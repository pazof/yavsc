using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yavsc.Interface;
using Yavsc.Interfaces;
using Yavsc.Org.Tests.Fakes;

namespace Yavsc.Org.Tests
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
            var factory = Assert.IsType<RecordingSmtpClientFactory>(
                scope.ServiceProvider.GetRequiredService<ISmtpClientFactory>());

            output.WriteLine("SendEMailSynchrone ...");
            mailSender.SendEmailAsync
          (
            _serverFixture.SiteSettings.Owner.Name,
            _serverFixture.SiteSettings.Owner.EMail,
            $"monthly email",
            "test boby monthly email").Wait();

            // Assert the SMTP roundtrip was short-circuited by the
            // recording fake installed in WebServerFixture: exactly
            // one client was created and it saw the expected sequence
            // of Connect → Authenticate → Send → Disconnect.
            var client = Assert.Single(factory.Created);
            Assert.Equal(
                new[]
                {
                    RecordingSmtpCallKind.Connect,
                    RecordingSmtpCallKind.Authenticate,
                    RecordingSmtpCallKind.Send,
                    RecordingSmtpCallKind.Disconnect,
                },
                client.Calls.Select(c => c.Kind).ToArray());
            Assert.Equal(_serverFixture.SiteSettings.Owner.EMail, client.LastSentMessage?.To.Mailboxes.First().Address);
        }
    }
}
