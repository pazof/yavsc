
using System.Threading.Tasks;

namespace Yavsc.Services
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync(SiteSettings siteSettings, SmtpSettings smtpSettings, string username, string email, string subject, string message);
    }
}
