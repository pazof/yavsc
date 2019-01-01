
using System.Threading.Tasks;
using Yavsc.Abstract.Manage;

namespace Yavsc.Services
{
    public interface IEmailSender
    {
        /// <summary>
        /// Sends en email.
        /// </summary>
        /// <param name="username">user name in database</param>
        /// <param name="email">user's email</param>
        /// <param name="subject">email subject</param>
        /// <param name="message">message</param>
        /// <returns>the message id</returns>
        Task<EmailSentViewModel> SendEmailAsync(string username, string email, string subject, string message);
    }
}
