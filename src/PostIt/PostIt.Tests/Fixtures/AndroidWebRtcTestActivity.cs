#if ANDROID
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Avalonia.Controls;
using Avalonia.Threading;
using PostIt.Android;
using PostIt.Android.Services;
using PostIt.Services;
using Yavsc.Abstract.Chat.Calls;
using Path = System.IO.Path;

namespace PostIt.Tests;

[Activity(Name = "fr.pschneider.postit.WebRtcTestActivity", Exported = true)]
[SupportedOSPlatform("android26.0")]
public sealed class AndroidWebRtcTestActivity : Activity
{
    protected override async void OnCreate(Bundle? state)
    {
        base.OnCreate(state);
        var result = Path.Combine(FilesDir!.AbsolutePath, "webrtc-test-result.txt");
        File.WriteAllText(result, "RUNNING");
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(75));
            await RunAsync(Intent?.GetIntExtra("port", 0) ?? 0, timeout.Token);
            File.WriteAllText(result, "PASS\nnative WebRTC media and cleanup complete");
        }
        catch (Exception error) { File.WriteAllText(result, "FAIL\n" + error); }
        finally { Finish(); }
    }

    private async Task RunAsync(int port, CancellationToken ct)
    {
        if (port is < 1024 or > 65535) throw new ArgumentOutOfRangeException(nameof(port));
        StartActivity(new Intent(this, typeof(MainActivity)).AddFlags(ActivityFlags.NewTask));
        while (!MainActivity.IsForeground || (Avalonia.Application.Current as App)?.View is null)
            await Task.Delay(100, ct);
        using var socket = new TcpClient();
        await socket.ConnectAsync("127.0.0.1", port, ct);
        using var stream = socket.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, false, 4096, leaveOpen: true);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false), 4096, leaveOpen: true) { AutoFlush = true };
        using var output = new SemaphoreSlim(1);
        var callbackFailure = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task EmitAsync(object value)
        {
            await output.WaitAsync(ct);
            try { await writer.WriteLineAsync(JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web))); }
            finally { output.Release(); }
        }
        void Emit(object value) => _ = EmitObservedAsync(value);
        async Task EmitObservedAsync(object value)
        {
            try { await EmitAsync(value); }
            catch (Exception error) { callbackFailure.TrySetException(error); }
        }
        var navigation = ((App)Avalonia.Application.Current!).View!.FindControl<NavigationPage>("NavRoot")
            ?? throw new InvalidOperationException("Main navigation unavailable.");
        AndroidWebRtcMedia? media = null;
        var attached = false;
        try
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                media = new AndroidWebRtcMedia();
                var grid = new Grid { RowDefinitions = new RowDefinitions("*,*") };
                grid.Children.Add(media.LocalVideo);
                Grid.SetRow(media.RemoteVideo, 1);
                grid.Children.Add(media.RemoteVideo);
                await navigation.PushAsync(new ContentPage { Content = grid });
                attached = true;
            });
            var peer = media ?? throw new InvalidOperationException("Native UI unavailable.");
            peer.Signal += signal => Emit(new { type = "signal", signal });
            peer.StatusChanged += status =>
            {
                Emit(new { type = "status", status });
                if (status.LocalVideoRendered) Emit(new { type = "rendered", target = "local", size = 1_000 });
                if (status.RemoteVideoRendered) Emit(new { type = "rendered", target = "remote", size = 1_000 });
            };
            while (true)
            {
                var read = reader.ReadLineAsync(ct).AsTask();
                var completed = await Task.WhenAny(read, peer.Completion, callbackFailure.Task);
                if (completed != read)
                {
                    await completed;
                    throw new IOException("Native WebRTC stopped unexpectedly.");
                }
                var line = await read;
                if (line is null) throw new IOException("Test signaling disconnected.");
                using var document = JsonDocument.Parse(line);
                var root = document.RootElement;
                switch (root.GetProperty("command").GetString())
                {
                    case "start":
                        var configuration = root.GetProperty("configuration").Deserialize<WebRtcMediaConfiguration>(
                            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                        var camera = AndroidLanCallMedia.GetDevices().Cameras.FirstOrDefault(c => c.Name == "Camera avant")
                            ?? throw new InvalidOperationException("Front camera required.");
                        await Dispatcher.UIThread.InvokeAsync(() => peer.StartAsync(
                            configuration with { Camera = camera.Value, Microphone = "microphone" }, ct));
                        break;
                    case "signal":
                        var signal = root.GetProperty("signal").Deserialize<CallSignal>(
                            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                        await peer.HandleSignalAsync(signal, ct);
                        break;
                    case "close":
                        await Dispatcher.UIThread.InvokeAsync(async () => await peer.DisposeAsync());
                        await EmitAsync(new { type = "closed" });
                        return;
                    default: throw new InvalidOperationException("Unknown test command.");
                }
            }
        }
        catch (Exception error)
        {
            await EmitAsync(new { type = "error", message = error.ToString() });
            throw;
        }
        finally
        {
            if (media is not null) await Dispatcher.UIThread.InvokeAsync(async () => await media.DisposeAsync());
            if (attached) await Dispatcher.UIThread.InvokeAsync(async () => await navigation.PopAsync());
        }
    }
}
#endif
