using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Abstract.Manage;

namespace test
{

    [Collection("EMaillingTeststCollection")]
    [Trait("regres", "no")]
    public class EMaillingTests : IClassFixture<ServerSideFixture>

    {
        ServerSideFixture _serverFixture;
        ITestOutputHelper output;
        public EMaillingTests(ServerSideFixture serverFixture, ITestOutputHelper output)
        {
            this.output = output;
            _serverFixture = serverFixture;
        }

        [Fact]
        public void SendEMailSynchrone()
        {

          AssertAsync.CompletesIn(2, () =>
              {
              output.WriteLine("SendEMailSynchrone ...");
              EmailSentViewModel mailSentInfo = _serverFixture._mailSender.SendEmailAsync
              (_serverFixture._siteSetup.Owner.Name, _serverFixture._siteSetup.Owner.EMail, $"monthly email", "test boby monthly email").Result;
              if (mailSentInfo==null) 
              _serverFixture._logger.LogError("No info on sending");
              else if (!mailSentInfo.Sent)
              _serverFixture._logger.LogError($"{mailSentInfo.ErrorMessage}");
              else 
              _serverFixture._logger.LogInformation($"mailId:{mailSentInfo.MessageId} \nto:{_serverFixture._siteSetup.Owner.Name}");
              Assert.NotNull(mailSentInfo);
              output.WriteLine($">>done with {mailSentInfo.EMail} {mailSentInfo.Sent} {mailSentInfo.MessageId} {mailSentInfo.ErrorMessage}");
              });
        }
    }
}
