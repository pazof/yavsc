using MailKit.Security;
using MimeKit;
using Yavsc.Interfaces;

namespace Yavsc.Services
{
    /// <summary>
    /// Production adapter from <see cref="Yavsc.Interfaces.ISmtpClient"/>
    /// to MailKit's <c>SmtpClient</c>. Stateless; the underlying
    /// MailKit client is created per <c>Connect</c> cycle by the
    /// factory and disposed by <c>MailSender</c>'s <c>using</c>.
    /// </summary>
    public sealed class MailKitSmtpClient : ISmtpClient
    {
        private readonly MailKit.Net.Smtp.SmtpClient _client;

        public MailKitSmtpClient()
        {
            _client = new MailKit.Net.Smtp.SmtpClient();
        }

        public int Timeout
        {
            get => _client.Timeout;
            set => _client.Timeout = value;
        }

        public void Connect(string host, int port, SecureSocketOptions options)
        {
            _client.Connect(host, port, options);
        }

        public void Authenticate(string userName, string password)
        {
            _client.Authenticate(userName, password);
        }

        public Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
        {
            return _client.SendAsync(message, cancellationToken);
        }

        public void Disconnect(bool quit)
        {
            _client.Disconnect(quit);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
