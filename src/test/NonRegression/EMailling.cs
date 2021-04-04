using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Abstract.Manage;

namespace test
{

    [Collection("EMaillingTeststCollection")]
    [Trait("regression", "II")]
    public class EMaillingTests : IClassFixture<ServerSideFixture>

    {
        readonly ServerSideFixture _serverFixture;
        readonly ITestOutputHelper output;
        readonly ILogger _logger;
        public EMaillingTests(ServerSideFixture serverFixture, ITestOutputHelper output)
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
              EmailSentViewModel mailSentInfo = _serverFixture.MailSender.SendEmailAsync
              (_serverFixture.SiteSetup.Owner.Name, _serverFixture.SiteSetup.Owner.EMail, $"monthly email", "test boby monthly email").Result;
              if (mailSentInfo==null) 
              _logger.LogError("No info on sending");
              else if (!mailSentInfo.Sent)
              _logger.LogError($"{mailSentInfo.ErrorMessage}");
              else 
              _logger.LogInformation($"mailId:{mailSentInfo.MessageId} \nto:{_serverFixture.SiteSetup.Owner.Name}");
              Assert.NotNull(mailSentInfo);
              output.WriteLine($">>done with {mailSentInfo.EMail} {mailSentInfo.Sent} {mailSentInfo.MessageId} {mailSentInfo.ErrorMessage}");
              });
        }
    }
}
