using Yavsc.Interfaces;

namespace Yavsc.Org.Tests.Fakes
{
    /// <summary>
    /// Test double for <see cref="ISmtpClientFactory"/>. Hands out
    /// <see cref="RecordingSmtpClient"/> instances and tracks them so
    /// tests can assert on every SMTP interaction.
    /// </summary>
    public sealed class RecordingSmtpClientFactory : ISmtpClientFactory
    {
        public List<RecordingSmtpClient> Created { get; } = new();

        public ISmtpClient CreateClient()
        {
            var client = new RecordingSmtpClient();
            Created.Add(client);
            return client;
        }
    }
}
