#nullable enable

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Chat.Calls;
using Yavsc.Server.Hubs;
using Yavsc.Settings;

namespace Yavsc.Services;

public sealed class CallSignalingService(
    IHubContext<ChatHub> hub,
    IOptions<AudioVideoCallOptions> options,
    TimeProvider clock,
    ILogger<CallSignalingService> logger) : BackgroundService
{
    private sealed class PendingCall(CallSession session, string callerConnection, string[] calleeConnections)
    {
        public CallSession Session { get; set; } = session;
        public string CallerConnection { get; } = callerConnection;
        public HashSet<string> CalleeConnections { get; } = [.. calleeConnections];
        public string? AcceptedConnection { get; set; }
        public bool Offered { get; set; }
        public bool Answered { get; set; }
        public int CandidateCount { get; set; }
        public HashSet<string> EndpointConnections { get; } = [];
        public string[] Connections => [CallerConnection, .. CalleeConnections];
    }

    private readonly SemaphoreSlim _gate = new(1);
    private readonly Dictionary<Guid, PendingCall> _calls = [];
    private readonly AudioVideoCallOptions _options = options.Value;

    public async Task<CallSession> InviteAsync(
        string room, string caller, string callee, string callerConnection,
        string[] calleeConnections, bool relayOnly, string transport = CallTransports.WebRtc)
    {
        EnsureEnabled();
        if (caller == callee || calleeConnections.Length == 0 ||
            calleeConnections.Contains(callerConnection))
            throw new HubException("Call participants are unavailable.");
        ValidateTransport(transport, relayOnly);
        await _gate.WaitAsync();
        try
        {
            await ExpireCoreAsync();
            if (_calls.Count >= 128 || _calls.Values.Any(c =>
                c.Session.Caller == caller || c.Session.Callee == caller ||
                c.Session.Caller == callee || c.Session.Callee == callee))
                throw new HubException("A participant is already in a call.");
            var session = new CallSession(Guid.NewGuid(), room, caller, callee, relayOnly,
                clock.GetUtcNow().AddSeconds(45), transport);
            var call = new PendingCall(session, callerConnection, calleeConnections);
            _calls.Add(session.Id, call);
            try
            {
                await hub.Clients.Clients(calleeConnections).SendAsync(CallHubMethods.Invitation, session);
            }
            catch
            {
                _calls.Remove(session.Id);
                logger.LogWarning("Could not deliver call invitation {CallId}", session.Id);
                throw;
            }
            return session;
        }
        finally { _gate.Release(); }
    }

    public async Task AcceptAsync(Guid id, string user, string connection, bool relayOnly)
    {
        EnsureEnabled();
        await _gate.WaitAsync();
        try
        {
            await ExpireCoreAsync();
            var call = Find(id);
            if (call.Session.Callee != user || !call.CalleeConnections.Contains(connection))
                throw new HubException("Only an invited device can accept this call.");
            if (call.AcceptedConnection != null)
                throw new HubException("This call was already accepted.");
            var requireRelay = call.Session.RelayOnly || relayOnly;
            ValidateTransport(call.Session.Transport, requireRelay);
            call.AcceptedConnection = connection;
            call.Session = call.Session with
            {
                RelayOnly = requireRelay,
                ExpiresAt = clock.GetUtcNow().AddHours(1)
            };
            var otherDevices = call.CalleeConnections.Where(c => c != connection).ToArray();
            call.CalleeConnections.IntersectWith([connection]);
            var accepted = new CallAccepted(call.Session, call.Session.Transport == CallTransports.LanSrtp
                ? [] : CreateIceServers(call.Session));
            try
            {
                if (otherDevices.Length > 0)
                    await hub.Clients.Clients(otherDevices).SendAsync(
                        CallHubMethods.Ended, new CallEnded(id, "answered-elsewhere"));
                await hub.Clients.Clients([call.CallerConnection, connection])
                    .SendAsync(CallHubMethods.Accepted, accepted);
            }
            catch
            {
                await FinishCoreAsync(call, "delivery-failed");
                throw;
            }
        }
        finally { _gate.Release(); }
    }

    public async Task SignalAsync(Guid id, string user, string connection, CallSignal signal)
    {
        EnsureEnabled();
        await _gate.WaitAsync();
        try
        {
            await ExpireCoreAsync();
            var call = Find(id);
            var caller = call.CallerConnection == connection && call.Session.Caller == user;
            var callee = call.AcceptedConnection == connection && call.Session.Callee == user;
            if ((!caller && !callee) || call.AcceptedConnection == null)
                throw new HubException("Media signaling requires acceptance on this device.");
            if (call.Session.Transport == CallTransports.LanSrtp)
            {
                ValidateTransport(call.Session.Transport, call.Session.RelayOnly);
                if (signal is null || signal.Kind != "rtp-endpoint" || signal.Endpoint?.IsValid() != true ||
                    signal.Sdp != null || signal.Candidate != null || signal.SdpMid != null ||
                    signal.SdpMLineIndex != null || call.EndpointConnections.Contains(connection))
                    throw new HubException("A LAN call requires one valid SRTP endpoint per accepted device.");
                var endpointTarget = caller ? call.AcceptedConnection : call.CallerConnection;
                await hub.Clients.Client(endpointTarget).SendAsync(CallHubMethods.SignalReceived, id, signal);
                call.EndpointConnections.Add(connection);
                return;
            }
            CallSignalValidator.Validate(signal, call.Session.RelayOnly);
            switch (signal.Kind)
            {
                case "offer":
                    if (!caller || call.Offered)
                        throw new HubException("Only the caller can send the initial offer once.");
                    break;
                case "answer":
                    if (!callee || !call.Offered || call.Answered)
                        throw new HubException("An answer requires the initial offer.");
                    break;
                case "candidate":
                    if ((caller && !call.Offered) || (callee && !call.Answered) || call.CandidateCount >= 256)
                        throw new HubException("ICE candidate is out of sequence or exceeds the call limit.");
                    break;
            }
            var target = caller ? call.AcceptedConnection : call.CallerConnection;
            await hub.Clients.Client(target).SendAsync(CallHubMethods.SignalReceived, id, signal);
            if (signal.Kind == "offer") call.Offered = true;
            if (signal.Kind == "answer") call.Answered = true;
            if (signal.Kind == "candidate") call.CandidateCount++;
        }
        finally { _gate.Release(); }
    }

    public async Task EndAsync(Guid id, string user, string connection)
    {
        await _gate.WaitAsync();
        try
        {
            var call = Find(id);
            if (!(call.Session.Caller == user && call.CallerConnection == connection) &&
                !(call.Session.Callee == user && (call.AcceptedConnection == connection ||
                    (call.AcceptedConnection == null && call.CalleeConnections.Contains(connection)))))
                throw new HubException("This device does not participate in the call.");
            await FinishCoreAsync(call, call.AcceptedConnection == null ? "cancelled-or-declined" : "hangup");
        }
        finally { _gate.Release(); }
    }

    public async Task<CallSession> GetSessionAsync(Guid id, string user, string connection)
    {
        EnsureEnabled();
        await _gate.WaitAsync();
        try
        {
            await ExpireCoreAsync();
            var call = Find(id);
            if (!(call.Session.Caller == user && call.CallerConnection == connection) &&
                !(call.Session.Callee == user && call.CalleeConnections.Contains(connection)))
                throw new HubException("This device does not participate in the call.");
            return call.Session;
        }
        finally { _gate.Release(); }
    }

    public async Task EndForUserAsync(string user, string reason, string? room = null)
    {
        await _gate.WaitAsync();
        try
        {
            foreach (var call in _calls.Values.Where(c =>
                (c.Session.Caller == user || c.Session.Callee == user) &&
                (room == null || c.Session.RoomName == room)).ToArray())
                await FinishCoreAsync(call, reason);
        }
        finally { _gate.Release(); }
    }

    public async Task DisconnectAsync(string connection)
    {
        await _gate.WaitAsync();
        try
        {
            foreach (var call in _calls.Values.ToArray())
            {
                if (call.CallerConnection == connection || call.AcceptedConnection == connection)
                    await FinishCoreAsync(call, "disconnected");
                else if (call.CalleeConnections.Remove(connection) && call.CalleeConnections.Count == 0)
                    await FinishCoreAsync(call, "disconnected");
            }
        }
        finally { _gate.Release(); }
    }

    public async Task ExpireAsync()
    {
        await _gate.WaitAsync();
        try { await ExpireCoreAsync(); }
        finally { _gate.Release(); }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5), clock);
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await ExpireAsync();
    }

    private PendingCall Find(Guid id) => _calls.TryGetValue(id, out var call)
        ? call : throw new HubException("Call not found or expired.");

    private async Task ExpireCoreAsync()
    {
        foreach (var call in _calls.Values.Where(c => c.Session.ExpiresAt <= clock.GetUtcNow()).ToArray())
            await FinishCoreAsync(call, "expired");
    }

    private async Task FinishCoreAsync(PendingCall call, string reason)
    {
        _calls.Remove(call.Session.Id);
        await hub.Clients.Clients(call.Connections)
            .SendAsync(CallHubMethods.Ended, new CallEnded(call.Session.Id, reason));
    }

    private void EnsureEnabled()
    {
        if (!_options.Enabled)
            throw new HubException("Audio/video calls are not enabled on this server.");
    }

    private void ValidateIceConfiguration(bool relayOnly)
    {
        if (_options.StunUrls.Any(url => !ValidUrl(url, "stun", "stuns")) ||
            _options.TurnUrls.Any(url => !ValidUrl(url, "turn", "turns")) ||
            (_options.TurnUrls.Length > 0 && string.IsNullOrWhiteSpace(_options.TurnSharedSecret)) ||
            (relayOnly && _options.TurnUrls.Length == 0))
            throw new HubException("ICE/TURN configuration is unavailable. No direct fallback is allowed.");
    }

    private void ValidateTransport(string transport, bool relayOnly)
    {
        if (transport == CallTransports.LanSrtp)
        {
            if (!_options.EnableLanCalls || relayOnly)
                throw new HubException("LAN calls must be enabled and cannot hide participant IP addresses.");
        }
        else if (transport == CallTransports.WebRtc)
            ValidateIceConfiguration(relayOnly);
        else
            throw new HubException("Unsupported call transport.");
    }

    private static bool ValidUrl(string url, params string[] schemes)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Length > 512 || url.Any(char.IsWhiteSpace))
            return false;
        var separator = url.IndexOf(':');
        if (separator <= 0 || !schemes.Contains(url[..separator]))
            return false;
        var endpoint = url[(separator + 1)..];
        if (endpoint.Contains('/') || !Uri.TryCreate("http://" + endpoint, UriKind.Absolute, out var uri) ||
            string.IsNullOrWhiteSpace(uri.Host) || uri.Port <= 0 ||
            uri.UserInfo.Length > 0 || uri.Fragment.Length > 0)
            return false;
        return uri.Query.Length == 0 || (url.StartsWith("turn", StringComparison.Ordinal) &&
            uri.Query is "?transport=udp" or "?transport=tcp");
    }

    private CallIceServer[] CreateIceServers(CallSession session)
    {
        List<CallIceServer> result = [];
        if (!session.RelayOnly && _options.StunUrls.Length > 0)
            result.Add(new CallIceServer(_options.StunUrls));
        if (_options.TurnUrls.Length > 0)
        {
            var username = $"{session.ExpiresAt.AddMinutes(5).ToUnixTimeSeconds()}:{Guid.NewGuid():N}";
            var credential = Convert.ToBase64String(HMACSHA1.HashData(
                Encoding.UTF8.GetBytes(_options.TurnSharedSecret!), Encoding.UTF8.GetBytes(username)));
            result.Add(new CallIceServer(_options.TurnUrls, username, credential));
        }
        return result.ToArray();
    }
}
