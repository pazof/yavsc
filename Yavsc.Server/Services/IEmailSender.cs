
using System.Threading.Tasks;

namespace Yavsc.Services
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync(string username, string email, string subject, string message);
    }
}
