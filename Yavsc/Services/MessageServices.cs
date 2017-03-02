
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System;
using Yavsc.Models.Messaging;
using Yavsc.Helpers;
using Microsoft.AspNet.Identity;
using Yavsc.Models;
using Yavsc.Models.Google.Messaging;
using System.Collections.Generic;
using Yavsc.Models.Haircut;

namespace Yavsc.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, IGoogleCloudMessageSender
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="googleSettings"></param>
        /// <param name="registrationId"></param>
        /// <param name="ev"></param>
        /// <returns>a MessageWithPayloadResponse,
        /// <c>bool somethingsent = (response.failure == 0 &amp;&amp; response.success > 0)</c>
        /// </returns>
        public async Task<MessageWithPayloadResponse> NotifyBookQueryAsync(GoogleAuthSettings googleSettings, IEnumerable<string> registrationIds, RdvQueryEvent ev)
        {
            MessageWithPayloadResponse response = null;
            await Task.Run(()=>{
                response = googleSettings.NotifyEvent<RdvQueryEvent>(registrationIds, ev);
            });
            return response;
        }

        public async Task<MessageWithPayloadResponse> NotifyEstimateAsync(GoogleAuthSettings googleSettings, IEnumerable<string> registrationIds, EstimationEvent ev)
        {
            MessageWithPayloadResponse response = null;
            await Task.Run(()=>{
                response = googleSettings.NotifyEvent<EstimationEvent>(registrationIds, ev);
            });
            return response;
        }

        public async Task<MessageWithPayloadResponse> NotifyHairCutQueryAsync(GoogleAuthSettings googleSettings,
         IEnumerable<string> registrationIds, HairCutQueryEvent ev)
        {
            MessageWithPayloadResponse response = null;
            await Task.Run(()=>{
                response = googleSettings.NotifyEvent<HairCutQueryEvent>(registrationIds, ev);
            });
            return response;
        }

        public Task<bool> SendEmailAsync(SiteSettings siteSettings, SmtpSettings smtpSettings, string email, string subject, string message)
        {
            try
            {
                MimeMessage msg = new MimeMessage();
                msg.From.Add(new MailboxAddress(
                    siteSettings.Owner.Name,
                siteSettings.Owner.Address));
                msg.To.Add(new MailboxAddress("", email));
                msg.Body = new TextPart("plain")
                {
                    Text = message
                };
                msg.Subject = subject;
                using (SmtpClient sc = new SmtpClient())
                {
                    sc.Connect(
                        smtpSettings.Host,
                        smtpSettings.Port,
                        SecureSocketOptions.None);
                    sc.Send(msg);
                }
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
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
