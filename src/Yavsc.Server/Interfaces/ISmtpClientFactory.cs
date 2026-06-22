namespace Yavsc.Interfaces
{
    /// <summary>
    /// Builds fresh <see cref="ISmtpClient"/> instances on demand.
    /// Per-call construction is required because <c>MailSender</c>
    /// owns its client with a <c>using</c> block: a singleton or
    /// scoped client would race across concurrent sends.
    /// </summary>
    public interface ISmtpClientFactory
    {
        ISmtpClient CreateClient();
    }
}
