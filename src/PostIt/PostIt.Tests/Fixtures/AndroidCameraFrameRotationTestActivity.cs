#if ANDROID
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Media;
using Android.OS;
using PostIt.Android.Services;
using Path = System.IO.Path;

namespace PostIt.Tests;

/// <summary>Debug-only synthetic encoder test; never opens the camera or contacts the API.</summary>
[Activity(Name = "fr.pschneider.postit.CameraFrameRotationTestActivity", Exported = true)]
[SupportedOSPlatform("android26.0")]
public sealed class AndroidCameraFrameRotationTestActivity : Activity
{
    protected override async void OnCreate(Bundle? state)
    {
        base.OnCreate(state);
        var resultPath = Path.Combine(FilesDir!.AbsolutePath, "camera-rotation-test-result.txt");
        File.WriteAllText(resultPath, "RUNNING");
        try
        {
            foreach (var rotation in new[] { 0, 90, 180, 270 })
                await VerifyRotationAsync(rotation);
            await VerifyPreviewOnlyAsync();
            File.WriteAllText(resultPath, "PASS\nrotations=0,90,180,270\nformat=MPEG-TS\npreview=RGBA,detach,reattach,letterbox,without-encoder,dynamic-rotation\nsource=synthetic");
        }
        catch (Exception ex)
        {
            File.WriteAllText(resultPath, $"FAIL\n{ex}");
        }
        finally { Finish(); }
    }

    private async Task VerifyPreviewOnlyAsync()
    {
        const int width = 640, height = 480;
        using var reader = ImageReader.NewInstance(640, 640, (ImageFormatType)Format.Rgba8888, 3)
            ?? throw new InvalidOperationException("Preview reader unavailable.");
        Exception? failure = null;
        using var rotator = await AndroidCameraFrameRotator.CreateAsync(null, width, height, 0,
            error => failure = error);
        await rotator.SetPreviewAsync(reader.Surface, reader.Width, reader.Height, error => failure = error);
        await rotator.StartAsync();
        using var paint = new Paint();
        Color[] colors = [Color.Red, Color.Green, Color.Blue, Color.Yellow];
        foreach (var rotation in new[] { 0, 90, 180, 270 })
        {
            await rotator.SetRotationAsync(rotation);
            var verified = 0;
            for (var frame = 0; frame < 20; frame++)
            {
                if (failure is not null) throw failure;
                var canvas = rotator.CameraSurface.LockCanvas(null)
                    ?? throw new InvalidOperationException("Synthetic preview canvas unavailable.");
                try
                {
                    for (var index = 0; index < colors.Length; index++)
                    {
                        paint.Color = colors[index];
                        var x = index % 2 * width / 2;
                        var y = index / 2 * height / 2;
                        canvas.DrawRect(x, y, x + width / 2, y + height / 2, paint);
                    }
                }
                finally
                {
                    rotator.CameraSurface.UnlockCanvasAndPost(canvas);
                    canvas.Dispose();
                }
                await Task.Delay(34);
                using var image = reader.AcquireLatestImage();
                if (image is null) continue;
                try
                {
                    if (frame < 3) continue;
                    VerifyPreview(image, rotation, rotation is 90 or 270 ? height : width,
                        rotation is 90 or 270 ? width : height);
                    verified++;
                }
                finally { image.Close(); }
            }
            if (verified < 3) throw new InvalidOperationException($"Preview-only rotation {rotation}: insufficient frames.");
        }
        await rotator.SetPreviewAsync(null);
    }

    private async Task VerifyRotationAsync(int rotation)
    {
        const int width = 640;
        const int height = 480;
        var outputWidth = rotation is 90 or 270 ? height : width;
        var outputHeight = rotation is 90 or 270 ? width : height;
        var path = Path.Combine(FilesDir!.AbsolutePath, $"camera-rotation-{rotation}.ts");
        void Stage(string stage) => File.WriteAllText(
            Path.Combine(FilesDir.AbsolutePath, "camera-rotation-test-result.txt"), $"RUNNING\nrotation={rotation}\nstage={stage}");
        Stage("prepare");
#pragma warning disable CS0618
        using var recorder = OperatingSystem.IsAndroidVersionAtLeast(31)
            ? new MediaRecorder(this) : new MediaRecorder();
#pragma warning restore CS0618
        recorder.SetVideoSource(VideoSource.Surface);
        recorder.SetOutputFormat(OutputFormat.Mpeg2Ts);
        recorder.SetVideoSize(outputWidth, outputHeight);
        recorder.SetVideoFrameRate(30);
        recorder.SetVideoEncodingBitRate(1200000);
        recorder.SetVideoEncoder(VideoEncoder.H264);
        recorder.SetOutputFile(path);
        recorder.Prepare();
        using var encoderSurface = recorder.Surface
            ?? throw new InvalidOperationException("Encoder surface unavailable.");
        using var preview = ImageReader.NewInstance(outputWidth + 160, outputHeight + 80,
            (ImageFormatType)Format.Rgba8888, 3)
            ?? throw new InvalidOperationException("Preview reader unavailable.");
        var previewFrames = 0;
        Exception? failure = null;
        var stopping = 0;
        Stage("initialize EGL");
        using (var rotator = await AndroidCameraFrameRotator.CreateAsync(
            encoderSurface, width, height, rotation, error =>
            {
                if (Volatile.Read(ref stopping) != 0) return;
                failure = error;
                File.WriteAllText(Path.Combine(FilesDir.AbsolutePath, "camera-rotation-test-result.txt"), $"FAIL\n{error}");
            }))
        {
            await rotator.SetPreviewAsync(preview.Surface, preview.Width, preview.Height,
                error => failure = error);
            Stage("start recorder");
            recorder.Start();
            Task? stop = null;
            try
            {
                await rotator.StartAsync();
                using var paint = new Paint();
                Color[] colors = [Color.Red, Color.Green, Color.Blue, Color.Yellow];
                Stage("draw frames");
                for (var frame = 0; frame < 90; frame++)
                {
                    if (failure is not null) throw failure;
                    if (frame == 10) await rotator.SetPreviewAsync(null);
                    if (frame == 15)
                        await rotator.SetPreviewAsync(preview.Surface, preview.Width, preview.Height,
                            error => failure = error);
                    if (frame == 45)
                    {
                        Volatile.Write(ref stopping, 1);
                        stop = Task.Run(recorder.Stop);
                    }
                    if (stop?.IsCompleted == true) break;
                    var canvas = rotator.CameraSurface.LockCanvas(null)
                        ?? throw new InvalidOperationException("Synthetic camera canvas unavailable.");
                    try
                    {
                        for (var i = 0; i < colors.Length; i++)
                        {
                            paint.Color = colors[i];
                            var x = i % 2 * width / 2;
                            var y = i / 2 * height / 2;
                            canvas.DrawRect(x, y, x + width / 2, y + height / 2, paint);
                        }
                    }
                    finally
                    {
                        rotator.CameraSurface.UnlockCanvasAndPost(canvas);
                        canvas.Dispose();
                    }
                    await Task.Delay(34);
                    using var image = preview.AcquireLatestImage();
                    if (image is not null)
                    {
                        try
                        {
                            VerifyPreview(image, rotation, outputWidth, outputHeight);
                            previewFrames++;
                        }
                        finally { image.Close(); }
                    }
                }
                if (failure is not null) throw failure;
                Stage("wait for recorder stop");
                await (stop ?? throw new InvalidOperationException("Recorder stop was not requested."))
                    .WaitAsync(TimeSpan.FromSeconds(10));
                if (previewFrames < 8)
                    throw new InvalidOperationException($"Too few preview frames: {previewFrames}.");
                await rotator.SetPreviewAsync(null);
            }
            finally
            {
                Stage("dispose EGL");
                rotator.Dispose();
            }
        }
        Stage("decode");
        using var retriever = new MediaMetadataRetriever();
        retriever.SetDataSource(path);
        using var bitmap = retriever.GetFrameAtTime(0, Option.ClosestSync)
            ?? throw new InvalidOperationException($"Cannot decode rotation {rotation}.");
        if (bitmap.Width != outputWidth || bitmap.Height != outputHeight)
            throw new InvalidOperationException($"Rotation {rotation}: dimensions {bitmap.Width}x{bitmap.Height}, expected {outputWidth}x{outputHeight}.");
        Color[] expected = rotation switch
        {
            90 => [Color.Blue, Color.Red, Color.Yellow, Color.Green],
            180 => [Color.Yellow, Color.Blue, Color.Green, Color.Red],
            270 => [Color.Green, Color.Yellow, Color.Red, Color.Blue],
            _ => [Color.Red, Color.Green, Color.Blue, Color.Yellow]
        };
        for (var i = 0; i < expected.Length; i++)
        {
            var actual = new Color(bitmap.GetPixel((i % 2 * 2 + 1) * outputWidth / 4,
                (i / 2 * 2 + 1) * outputHeight / 4));
            var target = expected[i];
            if (Math.Abs(actual.R - target.R) > 40 || Math.Abs(actual.G - target.G) > 40 || Math.Abs(actual.B - target.B) > 40)
                throw new InvalidOperationException($"Rotation {rotation}, quadrant {i}: {actual}, expected {target}.");
        }
        File.Delete(path);
    }

    private static void VerifyPreview(Image image, int rotation, int outputWidth, int outputHeight)
    {
        var plane = image.GetPlanes()![0];
        var buffer = plane.Buffer!;
        var scale = Math.Min((double)image.Width / outputWidth, (double)image.Height / outputHeight);
        var width = (int)(outputWidth * scale);
        var height = (int)(outputHeight * scale);
        Color[] expected = rotation switch
        {
            90 => [Color.Blue, Color.Red, Color.Yellow, Color.Green],
            180 => [Color.Yellow, Color.Blue, Color.Green, Color.Red],
            270 => [Color.Green, Color.Yellow, Color.Red, Color.Blue],
            _ => [Color.Red, Color.Green, Color.Blue, Color.Yellow]
        };
        for (var i = 0; i < expected.Length; i++)
        {
            var x = (image.Width - width) / 2 + (i % 2 * 2 + 1) * width / 4;
            var y = (image.Height - height) / 2 + (i / 2 * 2 + 1) * height / 4;
            var offset = y * plane.RowStride + x * plane.PixelStride;
            var actual = new Color(buffer.Get(offset) & 255, buffer.Get(offset + 1) & 255, buffer.Get(offset + 2) & 255);
            var target = expected[i];
            if (Math.Abs(actual.R - target.R) > 5 || Math.Abs(actual.G - target.G) > 5 || Math.Abs(actual.B - target.B) > 5)
                throw new InvalidOperationException($"Preview rotation {rotation}, quadrant {i}: {actual}, expected {target}.");
        }
        if (buffer.Get(0) != 0 || buffer.Get(1) != 0 || buffer.Get(2) != 0)
            throw new InvalidOperationException("Preview letterbox is not black.");
    }
}
#endif
