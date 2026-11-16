using Avalonia.Controls;
using Avalonia.Media.Imaging;
using PostIt.Views.Chat;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.ViewModels.Chat;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
[Collection(AvaloniaUiCollection.Name)]
public sealed class CallTransportRoutingTests(AvaloniaUiFixture ui)
{
    [Theory]
    [InlineData(CallTransports.LanSrtp)]
    [InlineData(CallTransports.WebRtc)]
    public Task Incoming_calls_are_handled_only_by_the_matching_transport(string transport)
        => ui.RunAsync(async () =>
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
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Audio_page_keeps_srtp_avatar_call_and_received_video_available_without_camera_controls()
        => ui.RunAsync(async () =>
    {
        var signaling = new Signaling();
        await using var lan = new LanCallViewModel(signaling, loadAvatar: (_, _) => Task.FromResult<Bitmap?>(null))
        {
            RemoteVideo = new Border()
        };
        var chat = new ChatViewModel(settings: new Settings()) { LanCall = lan, IsConnected = true };
        var page = new CallPage { DataContext = new AudioCallPageViewModel(chat) };
        var window = new Window { Content = page };
        try
        {
            window.Show();
            Assert.True(page.FindControl<StackPanel>("LanCallPanel")!.IsVisible);
            Assert.False(page.FindControl<ComboBox>("LanCamera")!.IsVisible);
            Assert.False(page.FindControl<Button>("LanVideoInvite")!.IsVisible);
            Assert.True(page.FindControl<Button>("LanAudioInvite")!.IsEffectivelyVisible);
            var video = page.FindControl<ContentControl>("LanRemoteVideo")!;
            Assert.True(video.IsEffectivelyVisible);
            Assert.Same(lan.RemoteVideo, video.Content);
        }
        finally { window.Close(); }
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Audio_page_shows_static_image_and_hides_video_and_srtp_controls()
        => ui.RunAsync(async () =>
    {
        var signaling = new Signaling();
        await using var webRtc = new WebRtcCallViewModel(signaling);
        var chat = new ChatViewModel(settings: new Settings()) { VideoCall = webRtc, IsConnected = true };
        signaling.Invite(new CallSession(Guid.NewGuid(), "General", "alice", "bob", false,
            DateTimeOffset.UtcNow.AddMinutes(1), DisplayMode: CallDisplayMode.AudioOnly));
        await WaitAsync(() => webRtc.Incoming);
        var page = new CallPage { DataContext = new AudioCallPageViewModel(chat) };
        var window = new Window { Content = page };
        try
        {
            window.Show();
            Assert.False(page.FindControl<ComboBox>("WebRtcCamera")!.IsVisible);
            Assert.False(page.FindControl<Button>("WebRtcVideoInvite")!.IsVisible);
            Assert.False(page.FindControl<StackPanel>("LanCallPanel")!.IsVisible);
            var fallback = page.FindControl<Image>("AudioFallbackImage")!;
            Assert.True(fallback.IsEffectivelyVisible);
            Assert.NotNull(fallback.Source);
            var displayedAvatar = page.FindControl<Image>("AudioAvatarImage")!;
            Assert.Null(displayedAvatar.Source);
            webRtc.RemoteAvatar = Assert.IsType<Bitmap>(fallback.Source);
            Assert.Same(webRtc.RemoteAvatar, displayedAvatar.Source);
        }
        finally { window.Close(); }
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Unknown_transport_is_rejected_once_without_starting_either_call()
        => ui.RunAsync(async () =>
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
    }, TestContext.Current.CancellationToken);

    [Theory]
    [InlineData(CallTransports.LanSrtp)]
    [InlineData(CallTransports.WebRtc)]
    public Task Chat_updates_both_rooms_and_disconnects_the_active_transport(string transport)
        => ui.RunAsync(async () =>
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
    }, TestContext.Current.CancellationToken);

    private static async Task WaitAsync(Func<bool> condition)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        while (!condition()) await Task.Delay(10, timeout.Token);
    }

    [Fact]
    public Task Selecting_a_room_member_prepares_both_transports_without_inviting()
        => ui.RunAsync(async () =>
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
        Assert.False(chat.OpenAudioCallCommand.CanExecute(null));
        chat.SelectedUser = "bob";
        Assert.True(chat.OpenCallCommand.CanExecute(null));
        Assert.True(chat.OpenAudioCallCommand.CanExecute(null));
        var page = chat.PrepareCallPage();
        Assert.Same(chat, page.Chat);
        Assert.Equal("bob", lan.Target);
        Assert.Equal("bob", webRtc.Target);
        Assert.False(lan.Busy);
        Assert.False(webRtc.Busy);
        chat.FocusedUsers.Remove("bob");
        Assert.Null(chat.SelectedUser);
        Assert.False(chat.OpenCallCommand.CanExecute(null));
        Assert.False(chat.OpenAudioCallCommand.CanExecute(null));
        chat.FocusedUsers.Add("bob");
        chat.SelectedUser = "bob";
        chat.SelectedRoom = "Other";
        Assert.Null(chat.SelectedUser);
        Assert.False(chat.OpenCallCommand.CanExecute(null));
    }, TestContext.Current.CancellationToken);

    [Theory]
    [InlineData(CallTransports.LanSrtp)]
    [InlineData(CallTransports.WebRtc)]
    public Task Leaving_text_chat_preserves_invitation_and_leaving_call_page_ends_it(string transport)
        => ui.RunAsync(async () =>
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
    }, TestContext.Current.CancellationToken);

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
        public Task<CallSession> InviteAudioAsync(string room, string user, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<CallSession> InviteLanAsync(string room, string user, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<CallSession> InviteLanAudioAsync(string room, string user, CancellationToken ct = default) =>
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
