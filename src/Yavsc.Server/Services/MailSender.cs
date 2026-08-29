using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Microsoft.AspNetCore.Identity;
using Yavsc.Interface;
using Yavsc.Interfaces;
using Yavsc.Settings;
using Yavsc.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Localization;
using System.Text.RegularExpressions;
using System.Web;

namespace Yavsc.Services
{
    public class MailSender : IEmailSender<ApplicationUser>, IEmailSender, ITrueEmailSender
    {

        private readonly IStringLocalizer<MailSender> localizer;
        readonly SiteSettings siteSettings;
        readonly SmtpSettings smtpSettings;
        private readonly ILogger logger;
        private readonly ISmtpClientFactory _smtpClientFactory;

        public MailSender(
            IOptions<SiteSettings> sitesOptions,
            IOptions<SmtpSettings> smtpOptions,
            ILoggerFactory loggerFactory,
            IStringLocalizer<MailSender> localizer,
            ISmtpClientFactory smtpClientFactory
            )
        {
            this.localizer = localizer;
            siteSettings = sitesOptions.Value;
            smtpSettings = smtpOptions.Value;
            logger = loggerFactory.CreateLogger<MailSender>();
            _smtpClientFactory = smtpClientFactory;
        }

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            throw new NotImplementedException();
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
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return SendEmailAsync(null, email, subject, htmlMessage);
        }

        internal static MailboxAddress BuildMailboxAddress(string? displayName, string? rawAddress)
        {
            if (string.IsNullOrWhiteSpace(rawAddress))
            {
                throw new FormatException("Email address is empty.");
            }

            var candidate = rawAddress.Trim();
            if (candidate.Contains('<') || candidate.Contains('>'))
            {
                var emailMatch = Regex.Match(candidate,
                    @"[A-Za-z0-9.!#$%&'*+/=?^_`{|}~-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

                if (emailMatch.Success)
                {
                    candidate = emailMatch.Value;
                }
                else
                {
                    candidate = candidate.Trim('<', '>', '"', '\'');
                }
            }

            candidate = candidate.Trim('"', '\'', '<', '>', ' ');
            candidate = candidate.Replace(" ", string.Empty);

            if (!MailboxAddress.TryParse(candidate, out var parsedAddress))
            {
                throw new FormatException($"Invalid email address '{rawAddress}'.");
            }

            var safeName = string.IsNullOrWhiteSpace(displayName)
                ? parsedAddress.Name
                : displayName.Trim();

            return new MailboxAddress(safeName ?? string.Empty, parsedAddress.Address);
        }

        public async Task<string> SendEmailAsync(string name, string email, string subject, string htmlMessage)
        {
            try
            {
                logger.LogInformation($"SendEmail for {email} : {subject}");
                MimeMessage msg = new();
                msg.From.Add(BuildMailboxAddress(siteSettings.Owner.Name, siteSettings.Owner.EMail));
                msg.To.Add(BuildMailboxAddress(name, email));
                TextPart text;
                msg.Body = text = new TextPart("html")
                {
                    Text = $"<html><body>{htmlMessage}</body></html>"
                };

                msg.Subject = subject;
                msg.MessageId = MimeKit.Utils.MimeUtils.GenerateMessageId(
                    siteSettings.Authority
                );
                using Yavsc.Interfaces.ISmtpClient sc = _smtpClientFactory.CreateClient();
                {
                    sc.Timeout = 30000;
                    sc.Connect(
                        smtpSettings.Host,
                        smtpSettings.Port,
                        SecureSocketOptions.Auto
                        );

                    if (smtpSettings.UserName != null)
                    {
                        sc.Authenticate(smtpSettings.UserName, smtpSettings.Password);
                    }

                    await sc.SendAsync(msg);
                    logger.LogInformation($"Sent : {msg.MessageId}");
                    sc.Disconnect(true);
                }
                return msg.MessageId;
            }
            catch (FormatException ex)
            {
                logger.LogError(ex, "Refusing to send email because the recipient or sender address is malformed. To={To}, From={From}", email, siteSettings.Owner.EMail);
                return string.Empty;
            }
            catch (SmtpCommandException ex)
            {
                logger.LogError(ex, "SMTP rejected the recipient or sender address. To={To}, Subject={Subject}, Status={Status}, Error={Error}", email, subject, ex.StatusCode, ex.Message);
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email. To={To}, Subject={Subject}", email, subject);
                throw;
            }
        }

        public void SendEmailFromCriteria(string Criteria)
        {
            throw new NotImplementedException();
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            var callbackUrl = siteSettings.ExternalUrl + "/Account/ResetPassword/" +
                    HttpUtility.UrlEncode(user.Id) + "/" + HttpUtility.UrlEncode(resetCode);

            await SendEmailAsync(user.UserName, user.Email,
            localizer["Reset Password"],
                    localizer["Please reset your password by "] + " <a href=\"" +
                    callbackUrl + "\" >following this link</a>");
            throw new NotImplementedException();
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            await SendEmailAsync(user.UserName, user.Email,
            localizer["Reset Password"],
                     localizer["Please reset your password by "] + " <a href=\"" +
                     resetLink + "\" >following this link</a>");
        }
    }
}
