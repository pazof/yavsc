#if ANDROID
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using PostIt.Android.Services;
using Path = System.IO.Path;

namespace PostIt.Tests;

[Activity(Name = "fr.pschneider.postit.PodcastBlurTestActivity", Exported = true)]
[SupportedOSPlatform("android26.0")]
public sealed class AndroidPodcastBlurTestActivity : Activity
{
    protected override async void OnCreate(Bundle? state)
    {
        base.OnCreate(state);
        var result = Path.Combine(FilesDir!.AbsolutePath, "podcast-blur-test-result.txt");
        File.WriteAllText(result, "RUNNING");
        try
        {
            await VerifyAsync();
            await VerifyRealModelAsync();
            await VerifyFailureAsync();
            File.WriteAllText(result, "PASS\nsegmentation=ONNX\nreal-model-preview=verified\npreview=verified\nencoded=created\nfailure=closed\nsource=synthetic");
        }
        catch (Exception ex) { File.WriteAllText(result, $"FAIL\n{ex}"); }
        finally { Finish(); }
    }

    private async Task VerifyAsync()
    {
        const int width = 640, height = 480;
        using (var segmenter = new PodcastPersonSegmenter(this))
        {
            var mask = segmenter.Segment(new byte[PodcastPersonSegmenter.Size * PodcastPersonSegmenter.Size * 4]);
            if (mask.Length != PodcastPersonSegmenter.Size * PodcastPersonSegmenter.Size)
                throw new InvalidOperationException("Masque de segmentation invalide.");
        }
        var path = Path.Combine(FilesDir!.AbsolutePath, "podcast-blur-test.ts");
#pragma warning disable CS0618
        using var recorder = OperatingSystem.IsAndroidVersionAtLeast(31) ? new MediaRecorder(this) : new MediaRecorder();
#pragma warning restore CS0618
        recorder.SetVideoSource(VideoSource.Surface);
        recorder.SetOutputFormat(OutputFormat.Mpeg2Ts);
        recorder.SetVideoSize(width, height);
        recorder.SetVideoFrameRate(30);
        recorder.SetVideoEncodingBitRate(1200000);
        recorder.SetVideoEncoder(VideoEncoder.H264);
        recorder.SetOutputFile(path);
        recorder.Prepare();
        using var surface = recorder.Surface ?? throw new InvalidOperationException("Encodeur indisponible.");
        using var preview = ImageReader.NewInstance(width, height, (ImageFormatType)Format.Rgba8888, 3)
            ?? throw new InvalidOperationException("Apercu indisponible.");
        Exception? failure = null;
        using var rotator = await AndroidCameraFrameRotator.CreateAsync(surface, width, height, 0,
            error => failure = error, this, _ => new SplitMask());
        await rotator.SetPreviewAsync(preview.Surface, width, height, error => failure = error);
        recorder.Start();
        await rotator.StartAsync();
        using var paint = new Paint();
        var frames = 0;
        Task? stop = null;
        for (var frame = 0; frame < 60; frame++)
        {
            if (failure is not null) throw failure;
            if (frame == 40) stop = Task.Run(recorder.Stop);
            if (stop?.IsCompleted == true) break;
            using var canvas = rotator.CameraSurface.LockCanvas(null)
                ?? throw new InvalidOperationException("Camera synthetique indisponible.");
            try
            {
                for (var x = 0; x < width; x += 8)
                {
                    paint.Color = x / 8 % 2 == 0 ? Color.Black : Color.White;
                    canvas.DrawRect(x, 0, x + 8, height, paint);
                }
            }
            finally { rotator.CameraSurface.UnlockCanvasAndPost(canvas); }
            await Task.Delay(70);
            using var image = preview.AcquireLatestImage();
            if (image is not null)
            {
                var plane = image.GetPlanes()![0];
                var buffer = plane.Buffer!;
                int Red(int x, int y) => buffer.Get(y * plane.RowStride + x * plane.PixelStride) & 255;
                var background = Math.Abs(Red(100, 240) - Red(108, 240));
                var foreground = Math.Abs(Red(500, 240) - Red(508, 240));
                if (frame > 5 && (background > 120 || foreground < 150))
                    throw new InvalidOperationException($"Apercu incorrect : decor={background}, personne={foreground}.");
                frames++;
                image.Close();
            }
        }
        if (failure is not null) throw failure;
        if (frames < 5) throw new InvalidOperationException($"Apercu insuffisant : {frames} images.");
        await (stop ?? throw new InvalidOperationException("Arret de l'encodeur non demande."))
            .WaitAsync(TimeSpan.FromSeconds(15));
        if (new FileInfo(path).Length == 0) throw new InvalidOperationException("Video encodee vide.");
    }

    private sealed class SplitMask : IPodcastPersonSegmenter
    {
        private readonly byte[] _mask = new byte[PodcastPersonSegmenter.Size * PodcastPersonSegmenter.Size];

        public SplitMask()
        {
            for (var y = 0; y < PodcastPersonSegmenter.Size; y++)
                for (var x = PodcastPersonSegmenter.Size / 2; x < PodcastPersonSegmenter.Size; x++)
                    _mask[y * PodcastPersonSegmenter.Size + x] = 255;
        }
        public byte[] Segment(byte[] rgba) => _mask;
        public void Dispose() { }
    }

    private async Task VerifyFailureAsync()
    {
        using var preview = ImageReader.NewInstance(640, 480, (ImageFormatType)Format.Rgba8888, 3)
            ?? throw new InvalidOperationException("Apercu indisponible.");
        var failed = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var rotator = await AndroidCameraFrameRotator.CreateAsync(null, 640, 480, 0,
            error => failed.TrySetResult(error), this, _ => new FailingMask());
        await rotator.SetPreviewAsync(preview.Surface, preview.Width, preview.Height,
            error => failed.TrySetResult(error));
        await rotator.StartAsync();
        using var canvas = rotator.CameraSurface.LockCanvas(null)
            ?? throw new InvalidOperationException("Camera synthetique indisponible.");
        try { canvas.DrawColor(Color.Red); }
        finally { rotator.CameraSurface.UnlockCanvasAndPost(canvas); }
        await failed.Task.WaitAsync(TimeSpan.FromSeconds(10));
        using var image = preview.AcquireLatestImage();
        if (image is not null) throw new InvalidOperationException("Une image non filtree a ete affichee apres l'echec.");
    }

    private async Task VerifyRealModelAsync()
    {
        using var preview = ImageReader.NewInstance(640, 480, (ImageFormatType)Format.Rgba8888, 3)
            ?? throw new InvalidOperationException("Apercu indisponible.");
        Exception? failure = null;
        using var rotator = await AndroidCameraFrameRotator.CreateAsync(null, 640, 480, 0,
            error => failure = error, this);
        await rotator.SetPreviewAsync(preview.Surface, preview.Width, preview.Height, error => failure = error);
        await rotator.StartAsync();
        var frames = 0;
        for (var i = 0; i < 15; i++)
        {
            if (failure is not null) throw failure;
            using var canvas = rotator.CameraSurface.LockCanvas(null)
                ?? throw new InvalidOperationException("Camera synthetique indisponible.");
            try { canvas.DrawColor(Color.Gray); }
            finally { rotator.CameraSurface.UnlockCanvasAndPost(canvas); }
            await Task.Delay(100);
            using var image = preview.AcquireLatestImage();
            if (image is not null) { frames++; image.Close(); }
        }
        if (failure is not null) throw failure;
        if (frames < 2) throw new InvalidOperationException($"Apercu avec modele reel insuffisant : {frames} images.");
    }

    private sealed class FailingMask : IPodcastPersonSegmenter
    {
        public byte[] Segment(byte[] rgba) => throw new InvalidOperationException("Echec de segmentation simule.");
        public void Dispose() { }
    }
}
#endif
