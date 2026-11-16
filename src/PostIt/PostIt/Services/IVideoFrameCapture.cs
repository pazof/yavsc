using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace PostIt.Services;

/// <summary>
/// Copies the frame displayed by a native video surface, for platforms where the LibVLC snapshot cannot read
/// hardware-decoded output. The image stays in memory.
/// </summary>
public interface IVideoFrameCapture
{
    /// <summary>Returns the displayed frame scaled to <paramref name="width"/> pixels, or null when no frame is available.</summary>
    Task<Bitmap?> CaptureFrameAsync(int width, CancellationToken ct);
}
