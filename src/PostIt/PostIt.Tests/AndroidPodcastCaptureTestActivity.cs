#if ANDROID
using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Avalonia.Threading;
using PostIt.Android;
using PostIt.Android.Services;
using PostIt.Services;

namespace PostIt.Tests;

/// <summary>Debug-only local encoder test; never connects to the podcast API.</summary>
[Activity(Name = "fr.pschneider.postit.PodcastCaptureTestActivity", Exported = true)]
[SupportedOSPlatform("android26.0")]
public sealed class AndroidPodcastCaptureTestActivity : Activity
{
    private bool _microphone = true;
    private bool _notificationStop;
    private int _cameraIndex;
    protected override async void OnCreate(Bundle? state)
    {
        base.OnCreate(state);
        _microphone = Intent?.GetStringExtra("microphone") != "false";
        _notificationStop = Intent?.GetStringExtra("notificationStop") == "true";
        _cameraIndex = Intent?.GetStringExtra("camera") == "front" ? 1 : 0;
        var resultPath = Path.Combine(FilesDir!.AbsolutePath, "podcast-capture-test-result.txt");
        File.WriteAllText(resultPath, "RUNNING");
        var result = new Bundle();
        try
        {
            await RunAsync(result);
            File.WriteAllText(resultPath, $"PASS\nsegments={result.GetInt("segments")}\ndurationSeconds={result.GetDouble("durationSeconds")}\naudio={result.GetBoolean("audio")}\ndirectory={result.GetString("directory")}\nbackground={result.GetString("background")}\norientation={result.GetString("orientation")}\ntextureTransform={result.GetString("textureTransform")}");
        }
        catch (Exception ex)
        {
            File.WriteAllText(resultPath, $"FAIL\n{ex}");
        }
        finally { Finish(); }
    }

    private async Task RunAsync(Bundle result)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            throw new PlatformNotSupportedException("Android 8 required.");
        var context = this;
        var intent = new Intent(context, typeof(MainActivity)).AddFlags(ActivityFlags.NewTask);
        StartActivity(intent);
        for (var i = 0; i < 100 && !MainActivity.IsForeground; i++) await Task.Delay(100);
        await Task.Delay(1000);
        var directory = Path.Combine(context.FilesDir!.AbsolutePath, "podcast-capture-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var devices = AndroidPodcastCapture.GetDevices();
        if (devices.Cameras.Count == 0) throw new InvalidOperationException("No emulator camera.");
        IPodcastCapture? capture = null;
        try
        {
            capture = await Dispatcher.UIThread.InvokeAsync(() => AndroidPodcastCapture.StartAsync(
                devices.Cameras[Math.Min(_cameraIndex, devices.Cameras.Count - 1)].Value,
                _microphone ? "microphone" : null, directory, CancellationToken.None));
            await Task.Delay(3000);
            var before = SegmentCount(directory);
            context.StartActivity(new Intent(Intent.ActionMain).AddCategory(Intent.CategoryHome).AddFlags(ActivityFlags.NewTask));
            await Task.Delay(9000);
            var after = SegmentCount(directory);
            if (capture.Completion.IsFaulted) await capture.Completion;
            if (capture is AndroidPodcastCapture androidCapture)
            {
                result.PutString("orientation", androidCapture.OrientationDiagnostics);
                result.PutString("textureTransform", androidCapture.TextureTransform);
            }
            if (after <= before) throw new InvalidOperationException($"Capture did not continue in background: {before} -> {after} segments.");
            if (_notificationStop)
            {
                using var stop = PendingIntent.GetService(context, 1,
                    new Intent(context, typeof(PodcastCaptureService)).SetAction(PodcastCaptureService.StopAction),
                    PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable)
                    ?? throw new InvalidOperationException("Notification stop action missing.");
                stop.Send();
            }
            else await capture.StopAsync().WaitAsync(TimeSpan.FromSeconds(15));
            await capture.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            var segments = HlsCapturePlaylist.Parse(await File.ReadAllTextAsync(Path.Combine(directory, "index.m3u8")));
            if (segments.Count < 3) throw new InvalidOperationException("Insufficient capture segments.");
            var duration = segments.Sum(segment => segment.DurationSeconds);
            if (duration < 8 || duration > 16)
                throw new InvalidOperationException($"Expected 8-16 seconds of capture, got {duration}.");
            using var extractor = new MediaExtractor();
            extractor.SetDataSource(Path.Combine(directory, segments[0].FileName));
            var video = false;
            var audio = false;
            for (var track = 0; track < extractor.TrackCount; track++)
            {
                using var format = extractor.GetTrackFormat(track);
                var mime = format.GetString(MediaFormat.KeyMime);
                video |= mime == "video/avc";
                audio |= mime == "audio/mp4a-latm";
            }
            if (!video || (_microphone && !audio))
                throw new InvalidOperationException($"Missing capture tracks: H.264={video}, AAC={audio}.");
            result.PutInt("segments", segments.Count);
            result.PutDouble("durationSeconds", duration);
            result.PutBoolean("audio", audio);
            result.PutString("directory", directory);
            result.PutString("background", $"segments {before} -> {after}");
        }
        finally
        {
            if (capture is not null) await capture.DisposeAsync();
        }
    }

    private static int SegmentCount(string directory)
    {
        var path = Path.Combine(directory, "index.m3u8");
        return File.Exists(path) ? HlsCapturePlaylist.Parse(File.ReadAllText(path)).Count : 0;
    }
}
#endif
