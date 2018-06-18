
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System;
using Yavsc.Models.Messaging;
using Microsoft.AspNet.Identity;
using Yavsc.Models;
using Yavsc.Models.Google.Messaging;
using System.Collections.Generic;
using Yavsc.Models.Haircut;
using Yavsc.Interfaces.Workflow;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Yavsc.Server.Helpers;
using Microsoft.Extensions.OptionsModel;

namespace Yavsc.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class MessageSender : IEmailSender, IGoogleCloudMessageSender
    {
        private ILogger _logger;
        SiteSettings siteSettings;
        SmtpSettings smtpSettings;
        GoogleAuthSettings googleSettings;

        public MessageSender(
            ILoggerFactory loggerFactory, 
            IOptions<SiteSettings> sitesOptions, 
            IOptions<SmtpSettings> smtpOptions,
            IOptions<GoogleAuthSettings> googleOptions
            )
        {
            _logger = loggerFactory.CreateLogger<MessageSender>();
            siteSettings = sitesOptions.Value;
            smtpSettings = smtpOptions.Value;
            googleSettings = googleOptions.Value;
        }

        public  async Task <MessageWithPayloadResponse> NotifyEvent<Event>
         (  IEnumerable<string> regids, Event ev)
           where Event : IEvent
        {
            if (ev == null)
                throw new Exception("Spécifier un évènement");

            if (ev.Sender == null)
                throw new Exception("Spécifier un expéditeur");

            if (regids == null )
                throw new NotImplementedException("Notify & No GCM reg ids");
            var raa = regids.ToArray();
            if (raa.Length<1)
                 throw new NotImplementedException("No GCM reg ids");
            var msg = new MessageWithPayload<Event>()
            {
                data = ev,
                registration_ids = regids.ToArray()
            };
            _logger.LogInformation("Sendding to Google : "+JsonConvert.SerializeObject(msg));
            try {
                using (var m = new SimpleJsonPostMethod("https://gcm-http.googleapis.com/gcm/send",$"key={googleSettings.ApiKey}")) {
                    return await m.Invoke<MessageWithPayloadResponse>(msg);
                }
            }
            catch (Exception ex) {
                throw new Exception ("Quelque chose s'est mal passé à l'envoi",ex);
            }
        }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="googleSettings"></param>
        /// <param name="registrationId"></param>
        /// <param name="ev"></param>
        /// <returns>a MessageWithPayloadResponse,
        /// <c>bool somethingsent = (response.failure == 0 &amp;&amp; response.success > 0)</c>
        /// </returns>
        public async Task<MessageWithPayloadResponse> NotifyBookQueryAsync( IEnumerable<string> registrationIds, RdvQueryEvent ev)
        {
            return await NotifyEvent<RdvQueryEvent>(registrationIds, ev);
        }

        public async Task<MessageWithPayloadResponse> NotifyEstimateAsync(IEnumerable<string> registrationIds, EstimationEvent ev)
        {
            return await NotifyEvent<EstimationEvent>(registrationIds, ev);
        }

        public async Task<MessageWithPayloadResponse> NotifyHairCutQueryAsync(
         IEnumerable<string> registrationIds, HairCutQueryEvent ev)
        {
            return await NotifyEvent<HairCutQueryEvent>(registrationIds, ev);
        }

        public Task<string> SendEmailAsync(string username, string email, string subject, string message)
        {
            string messageId=null;
            try
            {
                MimeMessage msg = new MimeMessage();
                msg.From.Add(new MailboxAddress(
                    siteSettings.Owner.Name,
                siteSettings.Owner.EMail));
                msg.To.Add(new MailboxAddress(username, email));
                msg.Body = new TextPart("plain")
                {
                    Text = message
                };
                msg.Subject = subject;
                msg.MessageId = MimeKit.Utils.MimeUtils.GenerateMessageId(
                    siteSettings.Authority
                );
                using (SmtpClient sc = new SmtpClient())
                {
                    sc.Connect(
                        smtpSettings.Host,
                        smtpSettings.Port,
                        SecureSocketOptions.None);
                    sc.Send(msg);
                    messageId = msg.MessageId;
                }
            }
            catch (Exception)
            {
                return Task.FromResult<string>(null);
            }
            return Task.FromResult(messageId);
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task<MessageWithPayloadResponse> NotifyAsync(
            IEnumerable<string> regids, IEvent yaev)
        {
            throw new NotImplementedException();
        }
        /* SMS with Twilio:
public Task SendSmsAsync(TwilioSettings twilioSettigns, string number, string message)
{
var Twilio = new TwilioRestClient(twilioSettigns.AccountSID, twilioSettigns.Token);
var result = Twilio.SendMessage( twilioSettigns.SMSAccountFrom, number, message);
// Status is one of Queued, Sending, Sent, Failed or null if the number is not valid
Trace.TraceInformation(result.Status);
// Twilio doesn't currently have an async API, so return success.

return Task.FromResult(result.Status != "Failed");

}  */
    }
}
