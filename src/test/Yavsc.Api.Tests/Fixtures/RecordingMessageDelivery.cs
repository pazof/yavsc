using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Yavsc.Interface;
using Yavsc.Server.Hubs;

namespace Yavsc.Api.Test.Fixtures;

public sealed class RecordingMessageDelivery : ITrueEmailSender, IHubContext<ChatHub>, IHubClients
{
    public record Email(string Recipient, string Subject, string Body);
    public record Push(string ConnectionId, string Method, object?[] Arguments);

    public ConcurrentQueue<Email> Emails { get; } = new();
    public ConcurrentQueue<Push> Pushes { get; } = new();
    IHubClients IHubContext<ChatHub>.Clients => this;
    IGroupManager IHubContext<ChatHub>.Groups => throw new NotSupportedException();
    public IClientProxy All => throw new NotSupportedException();
    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
    public IClientProxy Client(string connectionId) => new RecordingClient(this, connectionId);
    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => throw new NotSupportedException();
    public IClientProxy Group(string groupName) => throw new NotSupportedException();
    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
    public IClientProxy Groups(IReadOnlyList<string> groupNames) => throw new NotSupportedException();
    public IClientProxy User(string userId) => throw new NotSupportedException();
    public IClientProxy Users(IReadOnlyList<string> userIds) => throw new NotSupportedException();

    public Task<string> SendEmailAsync(string name, string email, string subject, string htmlMessage)
    {
        Emails.Enqueue(new Email(email, subject, htmlMessage));
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public void Clear()
    {
        Emails.Clear();
        Pushes.Clear();
    }

    private sealed class RecordingClient(RecordingMessageDelivery delivery, string connectionId) : IClientProxy
    {
        public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
        {
            delivery.Pushes.Enqueue(new Push(connectionId, method, args));
            return Task.CompletedTask;
        }
    }
}
