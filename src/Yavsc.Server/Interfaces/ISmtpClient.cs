using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace Yavsc.Interfaces
{
    /// <summary>
    /// Yavsc's own SMTP client surface. Narrower than
    /// <c>MailKit.Net.Smtp.ISmtpClient</c>: only the calls that
    /// <c>MailSender</c> actually makes. Production wires
    /// <c>MailKitSmtpClient</c>; tests wire a recording fake.
    /// </summary>
    public interface ISmtpClient : IDisposable
    {
        int Timeout { get; set; }

        void Connect(string host, int port, MailKit.Security.SecureSocketOptions options);

        void Authenticate(string userName, string password);

        Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default);

        void Disconnect(bool quit);
    }
}
