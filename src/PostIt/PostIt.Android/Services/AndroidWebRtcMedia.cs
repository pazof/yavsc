using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Platform;
using PostIt.Services;

namespace PostIt.Android.Services;

[SupportedOSPlatform("android26.0")]
internal sealed class AndroidWebRtcMedia : WebRtcNativeMedia
{
    private readonly CancellationTokenSource _lifetime = new();
    private readonly Peer _peer;
    private readonly Surface _local;
    private readonly Surface _remote;
    private readonly Task _poll;
    private readonly TaskCompletionSource _closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _disposed;

    public override Control LocalVideo => _local;
    public override Control RemoteVideo => _remote;

    public AndroidWebRtcMedia()
    {
        _peer = new Peer(MainActivity.Current ?? throw new InvalidOperationException("Activité Android indisponible."));
        _local = new Surface(_peer.GetView("localView"));
        _remote = new Surface(_peer.GetView("remoteView"));
        _poll = Task.Run(PollAsync);
    }

    protected override async Task StartCoreAsync(WebRtcMediaConfiguration configuration, CancellationToken ct)
    {
        var activity = MainActivity.Current ?? throw new InvalidOperationException("Activité Android indisponible.");
        await PodcastPermissions.EnsureAsync(activity, microphone: true, ct, notifications: false);
        if (!MainActivity.IsForeground)
            throw new InvalidOperationException("Revenez dans PostIt pour demarrer l'appel.");
        await Task.WhenAll(_local.Ready, _remote.Ready).WaitAsync(TimeSpan.FromSeconds(10), ct);
        await SendCommandAsync(StartCommand(configuration), ct);
    }

    protected override Task SendCommandAsync(string json, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _peer.Command(json);
        return Task.CompletedTask;
    }

    private async Task PollAsync()
    {
        var stats = Stopwatch.StartNew();
        var failed = false;
        try
        {
            while (!_lifetime.IsCancellationRequested)
            {
                for (var i = 0; i < 64 && _peer.Poll() is { } message; i++)
                {
                    if (message == "{\"type\":\"closed\"}") _closed.TrySetResult();
                    if (!failed)
                    {
                        try { ProcessMessage(message); }
                        catch (Exception error)
                        {
                            failed = true;
                            Fail(error);
                            if (_disposed) _closed.TrySetException(error);
                        }
                    }
                }
                if (!_disposed && !failed && Configuration is not null && stats.Elapsed >= TimeSpan.FromSeconds(1))
                {
                    _peer.Command("{\"command\":\"stats\"}");
                    stats.Restart();
                }
                await Task.Delay(20, _lifetime.Token);
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception error) { Fail(error); _closed.TrySetException(error); }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _peer.Command("{\"command\":\"close\"}");
            await _closed.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            _lifetime.Cancel();
            await _poll;
            _peer.Dispose();
            _lifetime.Dispose();
            Closed();
        }
    }

    private sealed class Surface(global::Android.Views.View view) : NativeControlHost
    {
        private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task Ready => _ready.Task;
        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            _ready.TrySetResult();
            return new AndroidViewControlHandle(view);
        }
    }

    // Only this small Java bridge crosses JNI. SDP and ICE callbacks stay in Java,
    // where the native SDK owns their lifetime and thread affinity.
    private sealed class Peer : IDisposable
    {
        private readonly Java.Lang.Class _type;
        private readonly Java.Lang.Object _instance;
        private readonly Java.Lang.Reflect.Method _command;
        private readonly Java.Lang.Reflect.Method _poll;

        public Peer(Context context)
        {
            _type = Java.Lang.Class.ForName("fr.pschneider.postit.WebRtcPeer")!;
            using var contextType = Java.Lang.Class.FromType(typeof(Context));
            using var constructor = _type.GetConstructor(contextType);
            _instance = constructor!.NewInstance(context)!;
            using var stringType = Java.Lang.Class.FromType(typeof(Java.Lang.String));
            _command = _type.GetMethod("command", stringType)!;
            _poll = _type.GetMethod("poll")!;
        }

        public global::Android.Views.View GetView(string method)
        {
            using var getter = _type.GetMethod(method);
            return getter!.Invoke(_instance)!.JavaCast<global::Android.Views.View>()!;
        }

        public void Command(string message)
        {
            using var value = new Java.Lang.String(message);
            _command.Invoke(_instance, value);
        }

        public string? Poll()
        {
            using var result = _poll.Invoke(_instance);
            return result?.ToString();
        }

        public void Dispose()
        {
            _command.Dispose();
            _poll.Dispose();
            _instance.Dispose();
            _type.Dispose();
        }
    }
}
