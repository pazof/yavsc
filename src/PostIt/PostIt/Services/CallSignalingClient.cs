using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Services;

public sealed class CallSignalingClient : ILanCallSignaling, IDisposable
{
    private readonly HubConnection _connection;
    private readonly IDisposable[] _subscriptions;
    private bool _disposed;

    public event Action<CallSession>? Invitation;
    public event Action<CallAccepted>? Accepted;
    public event Action<CallEnded>? Ended;
    public event Action<Guid, CallSignal>? Signal;
    public event Action? Interrupted;

    public CallSignalingClient(HubConnection connection)
    {
        _connection = connection;
        _subscriptions =
        [
            connection.On<CallSession>(CallHubMethods.Invitation, call => Invitation?.Invoke(call)),
            connection.On<CallAccepted>(CallHubMethods.Accepted, call => Accepted?.Invoke(call)),
            connection.On<CallEnded>(CallHubMethods.Ended, call => Ended?.Invoke(call)),
            connection.On<Guid, CallSignal>(CallHubMethods.SignalReceived,
                (id, signal) => Signal?.Invoke(id, signal))
        ];
        connection.Reconnecting += OnInterruptedAsync;
        connection.Closed += OnInterruptedAsync;
    }

    public Task<CallSession> InviteAsync(string room, string user, CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection.InvokeAsync<CallSession>(CallHubMethods.Invite, room, user, ct);
    }

    public Task<CallSession> InviteLanAsync(string room, string user, CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection.InvokeAsync<CallSession>(CallHubMethods.InviteLan, room, user, ct);
    }

    public Task AcceptAsync(Guid callId, CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection.InvokeAsync(CallHubMethods.Accept, callId, ct);
    }

    public Task EndAsync(Guid callId, CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection.InvokeAsync(CallHubMethods.End, callId, ct);
    }

    public Task SendAsync(Guid callId, CallSignal signal, CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection.InvokeAsync(CallHubMethods.Signal, callId, signal, ct);
    }

    private Task OnInterruptedAsync(Exception? error)
    {
        Interrupted?.Invoke();
        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_connection.State != HubConnectionState.Connected)
            throw new InvalidOperationException("La connexion de chat est interrompue ; l'appel ne peut pas continuer.");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connection.Reconnecting -= OnInterruptedAsync;
        _connection.Closed -= OnInterruptedAsync;
        foreach (var subscription in _subscriptions) subscription.Dispose();
    }
}
