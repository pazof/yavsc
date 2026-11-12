using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using PostIt.Views.Chat;
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

    [AvaloniaFact]
    public async Task Selecting_a_room_member_prepares_both_transports_without_inviting()
    {
        var signaling = new Signaling();
        await using var lan = new LanCallViewModel(signaling);
        await using var webRtc = new WebRtcCallViewModel(signaling);
        var chat = new ChatViewModel(settings: new Settings())
        {
            LanCall = lan, VideoCall = webRtc, IsConnected = true, SelectedRoom = "General",
            FocusedUsers = new()
        };
        chat.FocusedUsers.Add("bob");
        Assert.False(chat.OpenCallCommand.CanExecute(null));
        chat.SelectedUser = "bob";
        Assert.True(chat.OpenCallCommand.CanExecute(null));
        var page = chat.PrepareCallPage();
        Assert.Same(chat, page.Chat);
        Assert.Equal("bob", lan.Target);
        Assert.Equal("bob", webRtc.Target);
        Assert.False(lan.Busy);
        Assert.False(webRtc.Busy);
        chat.FocusedUsers.Remove("bob");
        Assert.Null(chat.SelectedUser);
        Assert.False(chat.OpenCallCommand.CanExecute(null));
        chat.FocusedUsers.Add("bob");
        chat.SelectedUser = "bob";
        chat.SelectedRoom = "Other";
        Assert.Null(chat.SelectedUser);
        Assert.False(chat.OpenCallCommand.CanExecute(null));
    }

    [AvaloniaTheory]
    [InlineData(CallTransports.LanSrtp)]
    [InlineData(CallTransports.WebRtc)]
    public async Task Leaving_text_chat_preserves_invitation_and_leaving_call_page_ends_it(string transport)
    {
        var signaling = new Signaling();
        await using var lan = new LanCallViewModel(signaling);
        await using var webRtc = new WebRtcCallViewModel(signaling);
        var chat = new ChatViewModel(settings: new Settings())
        {
            LanCall = lan, VideoCall = webRtc, IsConnected = true, SelectedRoom = "General",
            FocusedUsers = new()
        };
        var call = new CallSession(Guid.NewGuid(), "General", "alice", "bob", false,
            DateTimeOffset.UtcNow.AddMinutes(1), transport);
        signaling.Invite(call);
        await WaitAsync(() => lan.Incoming || webRtc.Incoming);
        Assert.True(chat.ShowCallCommand.CanExecute(null));
        var window = new Window { Content = new ChatPage { DataContext = chat } };
        try
        {
            window.Show();
            window.Content = new CallPage { DataContext = new CallPageViewModel(chat) };
            Assert.Empty(signaling.EndedCalls);
            Assert.True(lan.Incoming || webRtc.Incoming);
            window.Content = null;
            await WaitAsync(() => signaling.EndedCalls.Count != 0);
            Assert.Equal([call.Id], signaling.EndedCalls);
            Assert.False(lan.Busy);
            Assert.False(webRtc.Busy);
        }
        finally { window.Close(); }
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
