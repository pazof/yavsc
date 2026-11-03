using System;

namespace PostIt.Android.Services;

/// <summary>Serializes native launch ownership, including intents delivered after cancellation.</summary>
internal sealed class ChatServiceEpoch
{
    private readonly object _gate = new();
    private readonly string _process = Guid.NewGuid().ToString("N");
    private long _epoch;
    private bool _enabled;

    internal string Begin()
    {
        lock (_gate)
        {
            if (!_enabled) { ++_epoch; _enabled = true; }
            return Current;
        }
    }

    internal string? Restore()
    {
        lock (_gate)
        {
            // Only a genuinely fresh process may restore without a new user request.
            if (_epoch == 0) { ++_epoch; _enabled = true; }
            return _enabled ? Current : null;
        }
    }

    internal bool IsCurrent(string? epoch)
    {
        lock (_gate) return _enabled && epoch == Current;
    }

    internal bool Run(string? epoch, Action action)
    {
        lock (_gate)
        {
            if (!_enabled || epoch != Current) return false;
            action();
            return true;
        }
    }

    internal void Invalidate(Action stop)
    {
        lock (_gate)
        {
            ++_epoch;
            _enabled = false;
            stop();
        }
    }

    internal bool Cancel(string epoch, Action stop)
    {
        lock (_gate)
        {
            if (!_enabled || epoch != Current) return false;
            ++_epoch;
            _enabled = false;
            stop();
            return true;
        }
    }

    private string Current => $"{_process}:{_epoch}";
}
