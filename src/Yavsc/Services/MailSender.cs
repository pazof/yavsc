using System.Text;
using System;
using System.Net;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Yavsc.Abstract.Manage;
using Microsoft.AspNetCore.Identity.UI.Services;
using Yavsc.Interface;
using Yavsc.Settings;

namespace Yavsc.Services
{
    public class MailSender  : IEmailSender, ITrueEmailSender
    {
        readonly SiteSettings siteSettings;
        readonly SmtpSettings smtpSettings;
        private readonly ILogger logger;

        public MailSender(
            IOptions<SiteSettings> sitesOptions, 
            IOptions<SmtpSettings> smtpOptions,
            ILoggerFactory loggerFactory
            )
        {
            siteSettings = sitesOptions.Value;
            smtpSettings = smtpOptions.Value;
            logger = loggerFactory.CreateLogger<MailSender>();
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
        

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
           await SendEmailAsync("", email, subject, htmlMessage);
        }

        public async Task<string> SendEmailAsync(string name, string email, string subject, string htmlMessage)
        {
             logger.LogInformation($"SendEmail for {email} : {subject}");
            MimeMessage msg = new ();
            msg.From.Add(new MailboxAddress(siteSettings.Owner.Name,
            siteSettings.Owner.EMail));
            msg.To.Add(new MailboxAddress(name, email));
            msg.Body = new TextPart("html")
            {
                Text = htmlMessage
            };
            msg.Subject = subject;
            msg.MessageId = MimeKit.Utils.MimeUtils.GenerateMessageId(
                siteSettings.Authority
            );
            using (SmtpClient sc = new ())
            {
                sc.Connect(
                    smtpSettings.Server,
                    smtpSettings.Port,
                    SecureSocketOptions.Auto);
                
                if (smtpSettings.UserName!=null) {
                    NetworkCredential creds = new (
                    smtpSettings.UserName, smtpSettings.Password);
                    await sc.AuthenticateAsync(System.Text.Encoding.UTF8, creds, System.Threading.CancellationToken.None);
                }
                await sc.SendAsync(msg);
                logger.LogInformation($"Sent : {msg.MessageId}");
            }
            return msg.MessageId;
        }
    }
}
