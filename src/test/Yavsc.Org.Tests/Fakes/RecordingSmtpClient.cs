using MailKit.Security;
using MimeKit;
using Yavsc.Interfaces;

namespace Yavsc.Org.Tests.Fakes
{
    /// <summary>
    /// In-memory test double for <see cref="ISmtpClient"/>. Records
    /// every call for assertion and short-circuits the SMTP
    /// roundtrip.
    /// </summary>
    public sealed class RecordingSmtpClient : ISmtpClient
    {
        public List<RecordingSmtpCall> Calls { get; } = new();

        public int Timeout { get; set; }
        public string? LastConnectedHost { get; private set; }
        public int LastConnectedPort { get; private set; }
        public SecureSocketOptions LastConnectedOptions { get; private set; }
        public string? LastAuthenticatedUser { get; private set; }
        public MimeMessage? LastSentMessage { get; private set; }
        public bool WasDisconnected { get; private set; }
        public bool Disposed { get; private set; }

        public void Connect(string host, int port, SecureSocketOptions options)
        {
            LastConnectedHost = host;
            LastConnectedPort = port;
            LastConnectedOptions = options;
            Calls.Add(new RecordingSmtpCall(RecordingSmtpCallKind.Connect, host, port, options));
        }

        public void Authenticate(string userName, string password)
        {
            LastAuthenticatedUser = userName;
            Calls.Add(new RecordingSmtpCall(RecordingSmtpCallKind.Authenticate, userName, null, null));
        }

        public Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
        {
            LastSentMessage = message;
            Calls.Add(new RecordingSmtpCall(RecordingSmtpCallKind.Send, message.Subject, null, null));
            return Task.CompletedTask;
        }

        public void Disconnect(bool quit)
        {
            WasDisconnected = quit;
            Calls.Add(new RecordingSmtpCall(RecordingSmtpCallKind.Disconnect, null, null, null));
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    public enum RecordingSmtpCallKind
    {
        Connect,
        Authenticate,
        Send,
        Disconnect,
    }

    public readonly record struct RecordingSmtpCall(
        RecordingSmtpCallKind Kind,
        string? HostOrSubjectOrUser,
        int? Port,
        SecureSocketOptions? Options);
}
