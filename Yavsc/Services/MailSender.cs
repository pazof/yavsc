using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using MimeKit;
using Yavsc.Abstract.Manage;

namespace Yavsc.Services
{
    public class MailSender : IEmailSender
    {
        private ILogger _logger;
        SiteSettings siteSettings;
        SmtpSettings smtpSettings;

        public MailSender(
            ILoggerFactory loggerFactory, 
            IOptions<SiteSettings> sitesOptions, 
            IOptions<SmtpSettings> smtpOptions,
            IOptions<GoogleAuthSettings> googleOptions
            )
        {
            _logger = loggerFactory.CreateLogger<MailSender>();
            siteSettings = sitesOptions?.Value;
            smtpSettings = smtpOptions?.Value;
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
        

        public async Task<EmailSentViewModel> SendEmailAsync(string username, string email, string subject, string message)
        {
            EmailSentViewModel model = new EmailSentViewModel{ EMail = email };
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
                    await sc.SendAsync(msg);
                    model.MessageId = msg.MessageId;
                    model.Sent = true; // a duplicate info to remove from the view model, that equals to MessageId == null
                }
            }
            catch (Exception ex)
            {
                model.Sent = false;
                model.ErrorMessage = ex.Message;
                return model;
            }
            return model;
        }

    }
}
