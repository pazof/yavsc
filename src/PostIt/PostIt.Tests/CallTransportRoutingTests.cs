using Avalonia.Headless.XUnit;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.ViewModels.Chat;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
public sealed class CallTransportRoutingTests
{
    [AvaloniaTheory]
    [InlineData(CallTransports.LanSrtp)]
    [InlineData(CallTransports.WebRtc)]
    public async Task Incoming_calls_are_handled_only_by_the_matching_transport(string transport)
    {
        var signaling = new Signaling();
        await using var lan = new LanCallViewModel(signaling);
        await using var webRtc = new WebRtcCallViewModel(signaling);
        var call = new CallSession(Guid.NewGuid(), "General", "alice", "bob", false,
            DateTimeOffset.UtcNow.AddMinutes(1), transport);

        signaling.Invite(call);
        await WaitAsync(() => lan.Incoming || webRtc.Incoming);

        Assert.Equal(transport == CallTransports.LanSrtp, lan.Incoming);
        Assert.Equal(transport == CallTransports.WebRtc, webRtc.Incoming);
        Assert.Empty(signaling.EndedCalls);

        if (lan.Incoming) await lan.HangupAsync();
        else await webRtc.HangupAsync();
        Assert.Equal([call.Id], signaling.EndedCalls);
        Assert.False(lan.Busy);
        Assert.False(webRtc.Busy);
    }

    [AvaloniaFact]
    public async Task Unknown_transport_is_rejected_once_without_starting_either_call()
    {
        var signaling = new Signaling();
        await using var lan = new LanCallViewModel(signaling);
        await using var webRtc = new WebRtcCallViewModel(signaling);
        var call = new CallSession(Guid.NewGuid(), "General", "alice", "bob", false,
            DateTimeOffset.UtcNow.AddMinutes(1), "unknown");

        signaling.Invite(call);
        await WaitAsync(() => signaling.EndedCalls.Count != 0);

        Assert.Equal([call.Id], signaling.EndedCalls);
        Assert.False(lan.Busy);
        Assert.False(webRtc.Busy);
    }

    [AvaloniaTheory]
    [InlineData(CallTransports.LanSrtp)]
    [InlineData(CallTransports.WebRtc)]
    public async Task Chat_updates_both_rooms_and_disconnects_the_active_transport(string transport)
    {
        var signaling = new Signaling();
        var chat = new ChatViewModel(settings: new Settings());
        var lan = new LanCallViewModel(signaling);
        var webRtc = new WebRtcCallViewModel(signaling);
        chat.LanCall = lan;
        chat.VideoCall = webRtc;
        chat.SelectedRoom = "General";
        Assert.Equal("General", lan.Room);
        Assert.Equal("General", webRtc.Room);

        var call = new CallSession(Guid.NewGuid(), "General", "alice", "bob", false,
            DateTimeOffset.UtcNow.AddMinutes(1), transport);
        signaling.Invite(call);
        await WaitAsync(() => lan.Incoming || webRtc.Incoming);
        await chat.DisconnectAsync();

        Assert.Equal([call.Id], signaling.EndedCalls);
        Assert.Null(chat.LanCall);
        Assert.Null(chat.VideoCall);
        Assert.False(lan.Busy);
        Assert.False(webRtc.Busy);
        signaling.Invite(call);
        Assert.False(lan.Incoming);
        Assert.False(webRtc.Incoming);
    }

    private static async Task WaitAsync(Func<bool> condition)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        while (!condition()) await Task.Delay(10, timeout.Token);
    }

    private sealed class Signaling : IWebRtcCallSignaling
    {
        public event Action<CallSession>? Invitation;
        public event Action<CallAccepted>? Accepted;
        public event Action<CallEnded>? Ended;
        public event Action<Guid, CallSignal>? Signal;
        public event Action? Interrupted;
        public List<Guid> EndedCalls { get; } = [];

        public void Invite(CallSession call) => Invitation?.Invoke(call);
        public Task<CallSession> InviteAsync(string room, string user, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<CallSession> InviteLanAsync(string room, string user, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task AcceptAsync(Guid callId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task EndAsync(Guid callId, CancellationToken ct = default)
        {
            EndedCalls.Add(callId);
            return Task.CompletedTask;
        }
        public Task SendAsync(Guid callId, CallSignal signal, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }
}
