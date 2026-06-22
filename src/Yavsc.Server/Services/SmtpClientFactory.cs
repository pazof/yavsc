using Yavsc.Interfaces;

namespace Yavsc.Services
{
    /// <summary>
    /// Production <see cref="ISmtpClientFactory"/>: hands out a
    /// fresh <see cref="MailKitSmtpClient"/> per call. Stateless;
    /// safe to register as a singleton.
    /// </summary>
    public sealed class SmtpClientFactory : ISmtpClientFactory
    {
        public ISmtpClient CreateClient() => new MailKitSmtpClient();
    }
}
