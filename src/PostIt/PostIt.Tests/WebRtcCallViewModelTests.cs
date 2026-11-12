using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PostIt.Services;
using PostIt.ViewModels.Chat;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Tests;

[Trait("DependsOn", "Native")]
[Trait("Speed", "Slow")]
[Trait("Category", "non-reg")]
public sealed class WebRtcCallViewModelTests
{
    private static readonly CallIceServer[] Turn = [new(["turn:localhost:3478"], "temporary", "password")];
    private const string Sdp = "v=0\r\no=- 1 1 IN IP4 127.0.0.1\r\nc=IN IP4 0.0.0.0\r\nm=video 9 UDP/TLS/RTP/SAVPF 96\r\n";

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Outgoing_waits_for_acceptance_and_passes_server_policy_to_native_engine(bool relay)
    {
        var signaling = new Signaling(relay);
        var media = new Media();
        await using var vm = Configure(signaling, media);
        vm.AllowAddressDisclosure = !relay;
        await vm.InviteCommand.ExecuteAsync(null);
        Assert.True(vm.Busy);
        Assert.Null(media.Configuration);
        signaling.Ready();
        await WaitAsync(() => media.Configuration is not null);
        Assert.Equal(relay, media.Configuration!.RelayOnly);
        Assert.True(media.Configuration.Caller);
        Assert.Same(Turn, media.Configuration.IceServers);
        media.Emit(new("offer", Sdp: Sdp));
        await WaitAsync(() => signaling.Sent.Count == 1);
        signaling.Receive(new("answer", Sdp: Sdp));
        await WaitAsync(() => media.Received.Count == 1);
        await vm.HangupAsync();
        Assert.True(media.Disposed);
        Assert.Null(vm.LocalVideo);
        Assert.False(vm.Busy);
        Assert.Contains(signaling.Call.Id, signaling.EndedCalls);
    }

    [AvaloniaFact]
    public async Task Incoming_relay_needs_acceptance_but_never_requires_IP_disclosure()
    {
        var signaling = new Signaling(true);
        var media = new Media();
        await using var vm = Configure(signaling, media);
        signaling.Invite();
        await WaitAsync(() => vm.Incoming);
        Assert.False(vm.AllowAddressDisclosure);
        Assert.Null(media.Configuration);
        await vm.AcceptCommand.ExecuteAsync(null);
        await WaitAsync(() => media.Configuration is not null);
        Assert.False(media.Configuration!.Caller);
        media.Report(new("connected", "relay", 20, 20));
        await WaitAsync(() => vm.Status.Contains("IP masquee"));
    }

    [AvaloniaFact]
    public async Task Direct_call_without_consent_ends_before_any_capture()
    {
        var signaling = new Signaling(false);
        var media = new Media();
        await using var vm = Configure(signaling, media);
        await vm.InviteCommand.ExecuteAsync(null);
        Assert.Null(media.Configuration);
        Assert.False(vm.Busy);
        Assert.Contains("consentez", vm.Status);
        Assert.Single(signaling.EndedCalls);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Missing_TURN_or_policy_downgrade_never_creates_a_native_engine(bool downgrade)
    {
        var signaling = new Signaling(true);
        var created = false;
        await using var vm = new WebRtcCallViewModel(signaling, () =>
        {
            created = true;
            throw new InvalidOperationException("Should not create media");
        })
        { Camera = new("Camera", "camera"), Microphone = new("Mic", "default"), Target = "bob", Room = "General" };
        await vm.InviteCommand.ExecuteAsync(null);
        signaling.Ready(downgrade ? signaling.Call with { RelayOnly = false } : null,
            downgrade ? Turn : []);
        await WaitAsync(() => !vm.Busy);
        Assert.False(created);
        Assert.Single(signaling.EndedCalls);
    }

    [AvaloniaTheory]
    [InlineData("hangup")]
    [InlineData("interrupted")]
    [InlineData("ended")]
    [InlineData("background")]
    [InlineData("dispose")]
    public async Task All_termination_paths_cancel_in_progress_native_startup(string reason)
    {
        var signaling = new Signaling(true);
        var media = new Media { DelayStartup = true };
        await using var vm = Configure(signaling, media);
        await vm.InviteCommand.ExecuteAsync(null);
        signaling.Ready();
        await WaitAsync(() => media.Configuration is not null);
        switch (reason)
        {
            case "hangup": await vm.HangupAsync(); break;
            case "interrupted": signaling.Disconnect(); break;
            case "ended": signaling.End(); break;
            case "background": Platform.SuspendPlayback(); break;
            case "dispose": await vm.DisposeAsync(); break;
        }
        await WaitAsync(() => media.Disposed);
        Assert.False(vm.Busy);
        Assert.Null(vm.RemoteVideo);
        Assert.True(media.StartupCancelled);
    }

    [AvaloniaTheory]
    [InlineData("host")]
    [InlineData("srflx")]
    [InlineData(null)]
    public async Task Nonrelay_selected_path_is_rejected_and_media_is_stopped(string? candidateType)
    {
        var signaling = new Signaling(true);
        var media = new Media();
        await using var vm = Configure(signaling, media);
        await vm.InviteCommand.ExecuteAsync(null);
        signaling.Ready();
        await WaitAsync(() => media.Configuration is not null);
        media.Report(new("connected", candidateType, 20, 20));
        await WaitAsync(() => media.Disposed);
        Assert.Contains("chemin ICE", vm.Status);
    }

    [AvaloniaFact]
    public async Task Leaking_candidate_is_not_sent_and_fails_closed()
    {
        var signaling = new Signaling(true);
        var media = new Media();
        await using var vm = Configure(signaling, media);
        await vm.InviteCommand.ExecuteAsync(null);
        signaling.Ready();
        await WaitAsync(() => media.Configuration is not null);
        media.Emit(new("candidate", Candidate: "candidate:1 1 UDP 1 192.0.2.1 1234 typ host",
            SdpMid: "0", SdpMLineIndex: 0));
        await WaitAsync(() => media.Disposed);
        Assert.Empty(signaling.Sent);
        Assert.Contains("candidat", vm.Status);
    }

    //[AvaloniaFact]
    public async Task Native_failure_is_visible_and_ends_the_server_call()
    {
        var signaling = new Signaling(true);
        var media = new Media();
        await using var vm = Configure(signaling, media);
        await vm.InviteCommand.ExecuteAsync(null);
        signaling.Ready();
        await WaitAsync(() => media.Configuration is not null);
        media.Fail();
        await WaitAsync(() => media.Disposed);
        Assert.Contains("native failure", vm.Status);
        Assert.Single(signaling.EndedCalls);
    }

    [Theory]
    [InlineData("candidate:1 1 UDP 1 192.0.2.1 1234 typ host")]
    [InlineData("candidate:1 1 UDP 1 192.0.2.1 1234 typ srflx")]
    [InlineData("candidate:1 1 UDP 1 192.0.2.1 1234 typ relay raddr 198.51.100.1 rport 1234")]
    public void Privacy_rejects_nonrelay_or_related_address_leaks(string candidate) =>
        Assert.Throws<InvalidOperationException>(() => WebRtcPrivacy.ValidateSignal(
            new("candidate", Candidate: candidate, SdpMid: "0", SdpMLineIndex: 0), true));

  //  [Fact]
    public void Privacy_accepts_redacted_relay_and_rejects_SDP_address_leaks()
    {
        WebRtcPrivacy.ValidateSignal(new("candidate",
            Candidate: "candidate:1 1 UDP 1 192.0.2.1 1234 typ relay raddr 0.0.0.0 rport 0",
            SdpMid: "0", SdpMLineIndex: 0), true);
        WebRtcPrivacy.ValidateSignal(new("offer", Sdp: Sdp), true);
        Assert.Throws<InvalidOperationException>(() =>
            WebRtcPrivacy.ValidateSignal(new("offer", Sdp: Sdp.Replace("0.0.0.0", "198.51.100.1")), true));
    }

    public static bool NativeMediaAvailable => OperatingSystem.IsLinux() &&
        Environment.GetEnvironmentVariable("POSTIT_TEST_NATIVE_WEBRTC") == "1";

    [AvaloniaFact(Skip = "Requires Linux GStreamer/libnice and POSTIT_TEST_NATIVE_WEBRTC=1",
        SkipUnless = nameof(NativeMediaAvailable))]
    public async Task Linux_native_adapters_render_remote_bitmaps_and_receive_audio_in_both_directions()
    {
        await using var caller = new LinuxWebRtcMedia();
        await using var callee = new LinuxWebRtcMedia();
        var errors = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        void Forward(IWebRtcMedia destination, CallSignal signal) => Dispatcher.UIThread.Post(async () =>
        {
            try { await destination.HandleSignalAsync(signal, timeout.Token); }
            catch (Exception error) { errors.TrySetException(error); }
        });
        caller.Signal += signal => Forward(callee, signal);
        callee.Signal += signal => Forward(caller, signal);
        WebRtcMediaStatus? callerStatus = null, calleeStatus = null;
        caller.StatusChanged += status => callerStatus = status;
        callee.StatusChanged += status => calleeStatus = status;
        await callee.StartAsync(new([], false, false, "test", "test"), timeout.Token);
        await caller.StartAsync(new([], false, true, "test", "test"), timeout.Token);
        while (callerStatus is not { HasRemoteMedia: true } || calleeStatus is not { HasRemoteMedia: true } ||
               ((Image)caller.RemoteVideo).Source is not Bitmap || ((Image)callee.RemoteVideo).Source is not Bitmap)
        {
            if (errors.Task.IsCompleted) await errors.Task;
            if (caller.Completion.IsCompleted) await caller.Completion;
            if (callee.Completion.IsCompleted) await callee.Completion;
            await Task.Delay(20, timeout.Token);
        }
        Assert.Equal(640, ((Bitmap)((Image)caller.RemoteVideo).Source!).PixelSize.Width);
        Assert.Equal(360, ((Bitmap)((Image)callee.RemoteVideo).Source!).PixelSize.Height);
    }

    private static WebRtcCallViewModel Configure(Signaling signaling, Media media) => new(signaling, () => media)
    { Camera = new("Camera", "camera"), Microphone = new("Mic", "default"), Target = "bob", Room = "General" };

    private static async Task WaitAsync(Func<bool> condition)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        while (!condition()) await Task.Delay(10, timeout.Token);
    }

    private sealed class Media : IWebRtcMedia
    {
        private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Control LocalVideo { get; } = new Border();
        public Control RemoteVideo { get; } = new Border();
        public event Action<CallSignal>? Signal;
        public event Action<WebRtcMediaStatus>? StatusChanged;
        public Task Completion => _completion.Task;
        public WebRtcMediaConfiguration? Configuration { get; private set; }
        public bool Disposed { get; private set; }
        public bool StartupCancelled { get; private set; }
        public bool DelayStartup { get; init; }
        public List<CallSignal> Received { get; } = [];
        public async Task StartAsync(WebRtcMediaConfiguration configuration, CancellationToken ct)
        {
            Configuration = configuration;
            if (DelayStartup)
            {
                try { await Task.Delay(Timeout.Infinite, ct); }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                { StartupCancelled = true; throw; }
            }
        }
        public Task HandleSignalAsync(CallSignal signal, CancellationToken ct)
        { Received.Add(signal); return Task.CompletedTask; }
        public void Emit(CallSignal signal) => Signal?.Invoke(signal);
        public void Report(WebRtcMediaStatus status) => StatusChanged?.Invoke(status);
        public void Fail() => _completion.TrySetException(new IOException("native failure"));
        public ValueTask DisposeAsync()
        { Disposed = true; _completion.TrySetResult(); return ValueTask.CompletedTask; }
    }

    private sealed class Signaling(bool relay) : IWebRtcCallSignaling
    {
        public event Action<CallSession>? Invitation;
        public event Action<CallAccepted>? Accepted;
        public event Action<CallEnded>? Ended;
        public event Action<Guid, CallSignal>? Signal;
        public event Action? Interrupted;
        public CallSession Call { get; } = new(Guid.NewGuid(), "General", "alice", "bob", relay,
            DateTimeOffset.UtcNow.AddMinutes(1));
        public List<CallSignal> Sent { get; } = [];
        public List<Guid> EndedCalls { get; } = [];
        public Task<CallSession> InviteAsync(string room, string user, CancellationToken ct = default) => Task.FromResult(Call);
        public Task<CallSession> InviteLanAsync(string room, string user, CancellationToken ct = default) =>
            throw new InvalidOperationException("Legacy LAN transport must not be used");
        public Task AcceptAsync(Guid callId, CancellationToken ct = default) { Ready(); return Task.CompletedTask; }
        public Task EndAsync(Guid callId, CancellationToken ct = default)
        { EndedCalls.Add(callId); return Task.CompletedTask; }
        public Task SendAsync(Guid callId, CallSignal signal, CancellationToken ct = default)
        { Sent.Add(signal); return Task.CompletedTask; }
        public void Invite() => Invitation?.Invoke(Call);
        public void Ready(CallSession? session = null, CallIceServer[]? servers = null) =>
            Accepted?.Invoke(new(session ?? Call, servers ?? Turn));
        public void Receive(CallSignal signal) => Signal?.Invoke(Call.Id, signal);
        public void End() => Ended?.Invoke(new(Call.Id, "ended"));
        public void Disconnect() => Interrupted?.Invoke();
    }
}
