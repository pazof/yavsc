using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yavsc.Interface;
using Yavsc.Interfaces;
using Yavsc.Org.Tests.Fakes;
using Yavsc.ViewModels.Account;

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
            _logger = serverFixture.Logger!;
        }

        [Fact]
        public async Task SendEMailSynchrone()
        {

            using IServiceScope scope = _serverFixture.Services.CreateScope();
            ITrueEmailSender mailSender = scope.ServiceProvider.GetRequiredService<ITrueEmailSender>();
            var factory = Assert.IsType<RecordingSmtpClientFactory>(
                scope.ServiceProvider.GetRequiredService<ISmtpClientFactory>());

            output.WriteLine("SendEMailSynchrone ...");
            await mailSender.SendEmailAsync
          (
            _serverFixture.SiteSettings!.Owner.Name,
            _serverFixture.SiteSettings!.Owner.EMail,
            $"monthly email",
            "test boby monthly email");

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

        [Fact]
        public void RegisterModel_rejects_invalid_email_format()
        {
            var model = new RegisterModel
            {
                UserName = "alice",
                Email = "this is not an email",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var results = new List<ValidationResult>();
            var valid = Validator.TryValidateObject(
                model,
                new ValidationContext(model),
                results,
                validateAllProperties: true);

            Assert.False(valid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterModel.Email)));
        }

    }
}
