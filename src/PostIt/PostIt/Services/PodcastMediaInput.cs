using System;
using System.Runtime.InteropServices;
using System.Threading;
using LibVLCSharp.Shared;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.Services;

// LibVLC consumes a sequential transport stream; credentials stay in the existing HTTP transport.
public sealed class PodcastMediaInput(PodcastApiClient api, Guid id, bool live) : MediaInput
{
    private readonly CancellationTokenSource _cancellation = new();
    private byte[] _segment = Array.Empty<byte>();
    private int _offset;
    private int _sequence = -1;
    public event Action<string>? Failed;

    public override bool Open(out ulong size)
    {
        size = ulong.MaxValue;
        return true;
    }

    public override int Read(IntPtr buffer, uint length)
    {
        if (length == 0) return 0;
        try
        {
            var ct = _cancellation.Token;
            ct.ThrowIfCancellationRequested();
            while (_offset == _segment.Length)
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(TimeSpan.FromSeconds(20));
                var manifest = api.GetAsync(id, timeout.Token).GetAwaiter().GetResult();
                if (_sequence < 0)
                    _sequence = live ? Math.Max(0, manifest.Segments.Count - 3) : 0;
                if (_sequence < manifest.Segments.Count)
                {
                    _segment = api.DownloadSegmentAsync(id, _sequence, timeout.Token).GetAwaiter().GetResult();
                    if (_segment.Length == 0 || _segment.Length != manifest.Segments[_sequence].Bytes)
                        throw new InvalidOperationException("Segment podcast incomplet.");
                    _sequence++;
                    _offset = 0;
                    break;
                }
                if (manifest.Podcast.State != PodcastState.Live) return 0;
                if (ct.WaitHandle.WaitOne(700)) ct.ThrowIfCancellationRequested();
            }
            var count = (int)Math.Min(length, (uint)(_segment.Length - _offset));
            Marshal.Copy(_segment, _offset, buffer, count);
            _offset += count;
            return count;
        }
        catch (OperationCanceledException) when (_cancellation.IsCancellationRequested) { return -1; }
        // No managed exception may escape a native LibVLC callback.
        catch (Exception ex)
        {
            Failed?.Invoke($"Lecture du podcast interrompue : {ex.Message}");
            return -1;
        }
    }

    public override bool Seek(ulong offset) => false;
    public override void Close() { }
    public void Cancel() => _cancellation.Cancel();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }
        base.Dispose(disposing);
    }
}
