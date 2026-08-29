using System.ComponentModel.DataAnnotations;
using System.Globalization;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;
using Yavsc.Interface;
using Yavsc.Interfaces;
using Yavsc.Models.Relationship;
using Yavsc.Org.Tests.Fakes;
using Yavsc.Services;
using Yavsc.Settings;
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

        [Fact]
        public async Task SendEmailAsync_ignores_smtp_recipient_rejection()
        {
            var sender = new MailSender(
                Options.Create(new SiteSettings
                {
                    Title = "Test",
                    Authority = "example.com",
                    Owner = new StaticContact { Name = "Test Owner", EMail = "owner@example.com" }
                }),
                Options.Create(new SmtpSettings
                {
                    Host = "smtp.test.local",
                    Port = 465,
                    UserName = "test-user",
                    Password = "secret"
                }),
                NullLoggerFactory.Instance,
                new TestStringLocalizer(),
                new RejectingSmtpClientFactory());

            var result = await sender.SendEmailAsync(
                "Alice",
                "contact@pschneider.fr",
                "Welcome",
                "hello");

            Assert.Equal(string.Empty, result);
        }

        private sealed class RejectingSmtpClientFactory : ISmtpClientFactory
        {
            public Yavsc.Interfaces.ISmtpClient CreateClient() => new RejectingSmtpClient();
        }

        private sealed class RejectingSmtpClient : Yavsc.Interfaces.ISmtpClient
        {
            public int Timeout { get; set; }
            public void Connect(string host, int port, MailKit.Security.SecureSocketOptions options) { }
            public void Authenticate(string userName, string password) { }
            public Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
            {
                throw new SmtpCommandException(
                    SmtpErrorCode.RecipientNotAccepted,
                    SmtpStatusCode.MailboxUnavailable,
                    "Recipient address rejected: User unknown in local recipient table");
            }
            public void Disconnect(bool quit) { }
            public void Dispose() { }
        }

        private sealed class TestStringLocalizer : IStringLocalizer<MailSender>
        {
            public LocalizedString this[string name] => new(name, name);
            public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
                => Enumerable.Empty<LocalizedString>();

            public LocalizedString GetString(string name)
                => new(name, name);

            public LocalizedString GetString(string name, params object[] arguments)
                => new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

            public IStringLocalizer WithCulture(CultureInfo culture)
                => this;
        }

    }
}
