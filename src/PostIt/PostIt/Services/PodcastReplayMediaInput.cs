using System;
using System.Runtime.InteropServices;
using System.Threading;
using LibVLCSharp.Shared;
using Yavsc.Api.Client;

namespace PostIt.Services;

public sealed class PodcastReplayMediaInput : MediaInput
{
    private readonly PodcastApiClient _api;
    private readonly Guid _id;
    private readonly long _length;
    private readonly CancellationTokenSource _cancellation = new();
    private byte[] _buffer = [];
    private long _bufferStart;
    private long _position;
    public event Action<string>? Failed;

    public PodcastReplayMediaInput(PodcastApiClient api, Guid id, long length)
    {
        _api = api;
        _id = id;
        _length = length;
        CanSeek = true;
    }

    public override bool Open(out ulong size)
    {
        if (_length <= 0)
        {
            size = 0;
            Failed?.Invoke("Le MP4 du replay est vide.");
            return false;
        }
        _position = 0;
        _buffer = [];
        size = (ulong)_length;
        return true;
    }

    public override int Read(IntPtr buffer, uint lengthRequested)
    {
        if (lengthRequested == 0) return 0;
        try
        {
            _cancellation.Token.ThrowIfCancellationRequested();
            if (_position == _length) return 0;
            if (_position < _bufferStart || _position >= _bufferStart + _buffer.Length)
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_cancellation.Token);
                timeout.CancelAfter(TimeSpan.FromSeconds(20));
                _buffer = _api.DownloadReplayRangeAsync(_id, _position, 256 * 1024, _length, timeout.Token)
                    .GetAwaiter().GetResult();
                _bufferStart = _position;
            }
            var offset = checked((int)(_position - _bufferStart));
            var count = (int)Math.Min(lengthRequested, (uint)(_buffer.Length - offset));
            Marshal.Copy(_buffer, offset, buffer, count);
            _position += count;
            return count;
        }
        catch (OperationCanceledException) when (_cancellation.IsCancellationRequested) { return -1; }
        catch (Exception ex)
        {
            Failed?.Invoke($"Lecture du replay interrompue : {ex.Message}");
            return -1;
        }
    }

    public override bool Seek(ulong offset)
    {
        if (offset > (ulong)_length || _cancellation.IsCancellationRequested) return false;
        _position = (long)offset;
        return true;
    }

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
