using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using PostIt.Services;
using PostIt.ViewModels.Chat;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Tests;

public sealed class LanCallViewModelTests
{
    private static CallSession Session() => new(Guid.NewGuid(), "General", "alice", "bob", false,
        DateTimeOffset.UtcNow.AddMinutes(1), CallTransports.LanSrtp);

    private static LanCallViewModel Configure(Signaling signaling, Func<string, int, ILanCallMedia> create)
    {
        var vm = new LanCallViewModel(signaling, create)
        {
            Address = "127.0.0.1", Camera = new("Camera", "/dev/video0"),
            Microphone = new("Micro", "default"), AllowAddressDisclosure = true,
            Room = "General", Target = "bob"
        };
        vm.Addresses.Add("127.0.0.1");
        return vm;
    }

    [AvaloniaFact]
    public async Task No_capture_starts_before_explicit_acceptance_and_endpoint_exchange()
    {
        var signaling = new Signaling();
        var media = new FakeMedia();
        var created = 0;
        await using var vm = Configure(signaling, (_, _) => { created++; return media; });
        signaling.Invite(vm, signaling.Call);
        await WaitAsync(() => vm.Incoming);
        Assert.Equal(0, created);
        Assert.Empty(signaling.Sent);
        await vm.AcceptCommand.ExecuteAsync(null);
        await WaitAsync(() => vm.LocalVideo != null);
        Assert.Equal(1, created);
        Assert.False(media.Sending);
        Assert.Single(signaling.Sent);
        Assert.Equal("rtp-endpoint", signaling.Sent[0].Kind);
        signaling.RemoteEndpoint(signaling.Call.Id);
        await WaitAsync(() => media.Sending);
        Assert.Same(media.LocalVideo, vm.LocalVideo);
        Assert.Same(media.RemoteVideo, vm.RemoteVideo);
        await vm.HangupAsync();
        Assert.True(media.Disposed);
        Assert.False(vm.Busy);
        Assert.Null(vm.LocalVideo);
        Assert.Contains(signaling.Call.Id, signaling.EndedCalls);
    }

    [AvaloniaFact]
    public async Task Caller_waits_for_consent_and_interruption_releases_media_without_reconnecting()
    {
        var signaling = new Signaling();
        var media = new FakeMedia();
        var created = 0;
        await using var vm = Configure(signaling, (_, _) => { created++; return media; });
        await vm.InviteCommand.ExecuteAsync(null);
        Assert.True(vm.Busy);
        Assert.Equal(0, created);
        signaling.Ready();
        await WaitAsync(() => vm.LocalVideo != null);
        signaling.RemoteEndpoint(signaling.Call.Id);
        await WaitAsync(() => media.Sending);
        signaling.Disconnect();
        await WaitAsync(() => media.Disposed);
        Assert.False(vm.Busy);
        Assert.Null(vm.RemoteVideo);
        Assert.Contains("interrompue", vm.Status);
    }

    [AvaloniaFact]
    public async Task Missing_disclosure_consent_or_devices_blocks_the_invitation()
    {
        var signaling = new Signaling();
        await using var vm = Configure(signaling, (_, _) => throw new Exception("Must not capture"));
        vm.AllowAddressDisclosure = false;
        await vm.InviteCommand.ExecuteAsync(null);
        Assert.Equal(0, signaling.Invites);
        Assert.Contains("Confirmez", vm.Status);
        vm.AllowAddressDisclosure = true;
        vm.Camera = null;
        await vm.InviteCommand.ExecuteAsync(null);
        Assert.Equal(0, signaling.Invites);
        Assert.Contains("camera", vm.Status);
        Assert.False(vm.Busy);
    }

    [AvaloniaFact]
    public async Task Refusal_never_creates_media_and_late_signals_cannot_restart_capture()
    {
        var signaling = new Signaling();
        var created = 0;
        await using var vm = Configure(signaling, (_, _) => { created++; return new FakeMedia(); });
        signaling.Invite(vm, signaling.Call);
        await WaitAsync(() => vm.Incoming);
        await vm.HangupAsync();
        signaling.Ready();
        signaling.RemoteEndpoint(signaling.Call.Id);
        await Task.Delay(30, TestContext.Current.CancellationToken);
        Assert.Equal(0, created);
        Assert.False(vm.Busy);
    }

    [AvaloniaFact]
    public async Task Platform_devices_and_factory_are_used_and_backgrounding_ends_capture()
    {
        var previousDevices = Platform.GetLanCallDevices;
        var previousFactory = Platform.CreateLanCallMedia;
        var signaling = new Signaling();
        var media = new FakeMedia();
        var creations = 0;
        try
        {
            Platform.GetLanCallDevices = () => new(
                [new("Camera avant", "front"), new("Camera arriere", "rear")],
                [new("Sans microphone", ""), new("Microphone Android", "microphone")]);
            Platform.CreateLanCallMedia = (_, _) => { creations++; return media; };
            await using var vm = new LanCallViewModel(signaling);
            vm.RefreshDevicesCommand.Execute(null);
            Assert.Equal("front", vm.Camera?.Value);
            Assert.Equal("microphone", vm.Microphone?.Value);
            Assert.Single(vm.Microphones);
            Assert.Equal(0, creations);
            signaling.Invite(vm, signaling.Call);
            await WaitAsync(() => vm.Incoming);
            Assert.True(vm.CanConfigure);
            vm.RefreshDevicesCommand.Execute(null);
            Assert.Equal("front", vm.Camera?.Value);
            vm.Address = "127.0.0.1";
            if (!vm.Addresses.Contains(vm.Address)) vm.Addresses.Add(vm.Address);
            vm.AllowAddressDisclosure = true;
            vm.Room = "General";
            vm.Target = "bob";
            await vm.AcceptCommand.ExecuteAsync(null);
            await WaitAsync(() => vm.LocalVideo != null);
            Assert.Equal(1, creations);
            Assert.False(vm.CanConfigure);
            signaling.RemoteEndpoint(signaling.Call.Id);
            await WaitAsync(() => media.Sending);
            Platform.SuspendPlayback();
            await WaitAsync(() => media.Disposed);
            Assert.False(vm.Busy);
            Assert.Contains(signaling.Call.Id, signaling.EndedCalls);
            Platform.ResumePlayback();
            await Task.Delay(30, TestContext.Current.CancellationToken);
            Assert.Equal(1, creations);
        }
        finally
        {
            Platform.GetLanCallDevices = previousDevices;
            Platform.CreateLanCallMedia = previousFactory;
        }
    }

    [AvaloniaFact]
    public async Task Hangup_cancels_pending_native_start_without_waiting_for_startup_timeout()
    {
        var signaling = new Signaling();
        var media = new FakeMedia { DelayStartup = true };
        await using var vm = Configure(signaling, (_, _) => media);
        await vm.InviteCommand.ExecuteAsync(null);
        signaling.Ready();
        await WaitAsync(() => vm.LocalVideo != null);
        signaling.RemoteEndpoint(signaling.Call.Id);
        await WaitAsync(() => media.Sending);
        await vm.HangupAsync().WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        Assert.True(media.Disposed);
        Assert.False(vm.Busy);
        Assert.Contains(signaling.Call.Id, signaling.EndedCalls);
    }

    [AvaloniaFact]
    public async Task Media_failure_ends_the_call_and_disposes_capture()
    {
        var signaling = new Signaling();
        var media = new FakeMedia();
        await using var vm = Configure(signaling, (_, _) => media);
        await vm.InviteCommand.ExecuteAsync(null);
        signaling.Ready();
        await WaitAsync(() => vm.LocalVideo != null);
        media.Fail();
        await WaitAsync(() => media.Disposed);
        Assert.False(vm.Busy);
        Assert.Contains("test media failure", vm.Status);
        Assert.Contains(signaling.Call.Id, signaling.EndedCalls);
    }

    private static async Task WaitAsync(Func<bool> condition)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        while (!condition()) await Task.Delay(10, timeout.Token);
    }

    private sealed class FakeMedia : ILanCallMedia
    {
        private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Control LocalVideo { get; } = new Border();
        public Control RemoteVideo { get; } = new Border();
        public LanCallEndpoint Endpoint { get; } = new("127.0.0.1", 5004, new string('a', 32), new string('b', 28));
        public Task Completion => _completion.Task;
        public bool HasRemoteMedia => false;
        public bool Sending { get; private set; }
        public bool Disposed { get; private set; }
        public bool DelayStartup { get; init; }
        public async Task StartSendingAsync(LanCallEndpoint remote, string camera, string microphone, CancellationToken ct)
        {
            Sending = true;
            if (DelayStartup) await Task.Delay(Timeout.Infinite, ct);
        }
        public void Fail() => _completion.TrySetException(new IOException("test media failure"));
        public ValueTask DisposeAsync() { Disposed = true; _completion.TrySetResult(); return ValueTask.CompletedTask; }
    }

    private sealed class Signaling : ILanCallSignaling
    {
        public event Action<CallSession>? Invitation;
        public event Action<CallAccepted>? Accepted;
        public event Action<CallEnded>? Ended;
        public event Action<Guid, CallSignal>? Signal;
        public event Action? Interrupted;
        public CallSession Call { get; } = Session();
        public List<CallSignal> Sent { get; } = [];
        public List<Guid> EndedCalls { get; } = [];
        public int Invites { get; private set; }
        public Task<CallSession> InviteLanAsync(string room, string user, CancellationToken ct = default)
        { Invites++; return Task.FromResult(Call); }
        public Task AcceptAsync(Guid id, CancellationToken ct = default) { Ready(); return Task.CompletedTask; }
        public Task EndAsync(Guid id, CancellationToken ct = default)
        { EndedCalls.Add(id); Ended?.Invoke(new(id, "hangup")); return Task.CompletedTask; }
        public Task SendAsync(Guid id, CallSignal signal, CancellationToken ct = default)
        { Sent.Add(signal); return Task.CompletedTask; }
        public void Invite(LanCallViewModel _, CallSession call) => Invitation?.Invoke(call);
        public void Ready() => Accepted?.Invoke(new(Call, []));
        public void RemoteEndpoint(Guid id) => Signal?.Invoke(id, new("rtp-endpoint",
            Endpoint: new("127.0.0.1", 5006, new string('c', 32), new string('d', 28))));
        public void Disconnect() => Interrupted?.Invoke();
    }
}
