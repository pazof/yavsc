#if ANDROID
using System;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Avalonia.Controls;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using PostIt.Android;
using PostIt.Android.Services;
using PostIt.Services;
using Path = System.IO.Path;

namespace PostIt.Tests;

/// <summary>Debug-only physical camera/SRTP test; no account, API or media recording.</summary>
[Activity(Name = "fr.pschneider.postit.LanCallTestActivity", Exported = true)]
[SupportedOSPlatform("android26.0")]
public sealed class AndroidLanCallTestActivity : Activity
{
    protected override async void OnCreate(Bundle? state)
    {
        base.OnCreate(state);
        var result = Path.Combine(FilesDir!.AbsolutePath, "lan-call-test-result.txt");
        File.WriteAllText(result, "RUNNING");
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(55));
            var outcome = await RunAsync(result, Intent?.GetStringExtra("wrongKey") == "true", timeout.Token);
            File.WriteAllText(result, outcome);
        }
        catch (Exception error) { File.WriteAllText(result, "FAIL\n" + error); }
        finally { Finish(); }
    }

    private async Task<string> RunAsync(string result, bool wrongKey, CancellationToken ct)
    {
        StartActivity(new Intent(this, typeof(MainActivity)).AddFlags(ActivityFlags.NewTask));
        while (!MainActivity.IsForeground || (Avalonia.Application.Current as App)?.View == null)
            await Task.Delay(100, ct);
        await Task.Delay(1000, ct);
        var fixture = Path.Combine(FilesDir!.AbsolutePath, "lan-call-fixture.ts");
        if (!File.Exists(fixture)) throw new IOException("Explicit H.264/AAC MPEG-TS fixture required.");
        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        await using var peer = new LibVlcLanCallMedia("127.0.0.1", 35102, headless: true);
        using var decoded = new DecodedFrames(peer.Receiver);
        AndroidLanCallMedia? call = null;
        Task? pump = null;
        using var sending = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var source = File.OpenRead(fixture);
        var app = (App)Avalonia.Application.Current!;
        var navigation = app.View!.FindControl<NavigationPage>("NavRoot")
            ?? throw new InvalidOperationException("Main navigation unavailable.");
        var attached = false;
        try
        {
            await peer.StartReceivingAsync(ct);
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                call = new AndroidLanCallMedia("127.0.0.1", 35100);
                var grid = new Grid { RowDefinitions = new RowDefinitions("*,*") };
                grid.Children.Add(call.LocalVideo);
                Grid.SetRow(call.RemoteVideo, 1);
                grid.Children.Add(call.RemoteVideo);
                await navigation.PushAsync(new ContentPage { Content = grid });
                attached = true;
            });
            if (call == null) throw new InvalidOperationException("Call UI not created.");
            File.WriteAllText(result, "RUNNING\nstage=return source pump");
            pump = Task.Run(async () =>
            {
                var buffer = new byte[1316];
                while (await source.ReadAsync(buffer, sending.Token) is var count && count > 0)
                {
                    await pipe.WriteAsync(buffer.AsMemory(0, count), sending.Token);
                    await Task.Delay(4, sending.Token);
                }
            }, ct);
            File.WriteAllText(result, "RUNNING\nstage=return SRTP sender");
            await peer.StartEncodedSendingAsync(call.Endpoint,
                pipe.ClientSafePipeHandle.DangerousGetHandle().ToInt32(), ct);
            File.WriteAllText(result, "RUNNING\nstage=enumerating cameras");
            var devices = AndroidLanCallMedia.GetDevices();
            var front = devices.Cameras.FirstOrDefault(c => c.Name == "Camera avant")
                ?? throw new InvalidOperationException("Front camera required; no rear-camera fallback.");
            var endpoint = wrongKey ? peer.Endpoint with { Key = new string('0', 32) } : peer.Endpoint;
            File.WriteAllText(result, "RUNNING\nstage=front camera and SRTP");
            await Dispatcher.UIThread.InvokeAsync(() => call.StartSendingAsync(endpoint, front.Value, "microphone", ct));
            File.WriteAllText(result, "RUNNING\nstage=receiving camera and return media");
            var elapsed = Stopwatch.StartNew();
            while (elapsed.Elapsed < TimeSpan.FromSeconds(wrongKey ? 10 : 25))
            {
                if (call.Completion.IsCompleted) await call.Completion;
                if (peer.Completion.IsCompleted) await peer.Completion;
                if (!wrongKey && elapsed.Elapsed >= TimeSpan.FromSeconds(10) &&
                    decoded.Frames >= 100 && decoded.Samples > 144000 && call.HasRemoteMedia) break;
                await Task.Delay(100, ct);
            }
            if (!call.HasRemoteMedia) throw new IOException("Android did not decode return audio/video.");
            if (wrongKey ? decoded.Frames != 0 || decoded.Samples != 0
                : decoded.Frames < 100 || decoded.Samples <= 144000)
                throw new IOException($"Unexpected camera decode: frames={decoded.Frames}, samples={decoded.Samples}.");
            return $"PASS\nfrontCamera=true\nwrongKey={wrongKey}\n" +
                $"cameraFrames={decoded.Frames}\ncameraAudioSamples={decoded.Samples}\nelapsedMs={elapsed.ElapsedMilliseconds}\nreturnAudioVideo=true\n" +
                "transport=SRTP/UDP loopback\npreview=attached\nsource=camera+microphone,return=explicit fixture\ncleanup=complete";
        }
        finally
        {
            sending.Cancel();
            File.WriteAllText(result, "RUNNING\nstage=stop return source");
            if (pump != null)
            {
                try { await pump; }
                catch (System.OperationCanceledException) when (sending.IsCancellationRequested) { }
            }
            File.WriteAllText(result, "RUNNING\nstage=dispose call");
            if (call != null) await Dispatcher.UIThread.InvokeAsync(async () => await call.DisposeAsync());
            File.WriteAllText(result, "RUNNING\nstage=stop return peer");
            await peer.StopAsync();
            source.Dispose();
            File.WriteAllText(result, "RUNNING\nstage=detach test page");
            if (attached) await Dispatcher.UIThread.InvokeAsync(async () => await navigation.PopAsync());
        }
    }

    private sealed class DecodedFrames : IDisposable
    {
        private readonly MediaPlayer _player;
        private readonly IntPtr _buffer = Marshal.AllocHGlobal(640 * 640 * 4);
        private int _frames;
        private long _samples;
        public int Frames => Volatile.Read(ref _frames);
        public long Samples => Interlocked.Read(ref _samples);
        public DecodedFrames(MediaPlayer player)
        {
            _player = player;
            player.SetVideoFormat("RV32", 640, 640, 640 * 4);
            player.SetVideoCallbacks((_, planes) =>
            {
                Marshal.WriteIntPtr(planes, _buffer);
                return IntPtr.Zero;
            }, null, (_, _) => Interlocked.Increment(ref _frames));
            player.SetAudioFormat("S16N", 48000, 1);
            player.SetAudioCallbacks((_, _, count, _) => Interlocked.Add(ref _samples, count),
                null, null, null, null);
        }
        public void Dispose() { _player.Stop(); Marshal.FreeHGlobal(_buffer); }
    }
}
#endif
