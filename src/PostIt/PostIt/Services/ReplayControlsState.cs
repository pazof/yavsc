using System;

namespace PostIt.Services;

public sealed class ReplayControlsState(TimeProvider? clock = null)
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;
    private DateTimeOffset _lastInteraction = (clock ?? TimeProvider.System).GetUtcNow();
    private long? _pendingPreview;
    private long? _appliedPreview;
    public bool IsInteracting { get; set; }
    public void Show() => _lastInteraction = _clock.GetUtcNow();

    /// <summary>Records the latest position requested while dragging; older requests are dropped.</summary>
    public void RequestPreview(long milliseconds) => _pendingPreview = Math.Max(0, milliseconds);

    /// <summary>Returns the latest unapplied preview position, if any.</summary>
    public bool TryTakePreview(out long milliseconds)
    {
        milliseconds = _pendingPreview ?? 0;
        var take = _pendingPreview is { } pending && pending != _appliedPreview;
        _pendingPreview = null;
        if (take) _appliedPreview = milliseconds;
        return take;
    }

    public void EndPreview() => _pendingPreview = _appliedPreview = null;
    public bool IsVisible(bool playing, bool focused)
        => !playing || focused || IsInteracting || _clock.GetUtcNow() - _lastInteraction < TimeSpan.FromSeconds(3);

    public static string FormatTime(long milliseconds)
    {
        var time = TimeSpan.FromMilliseconds(Math.Max(0, milliseconds));
        return time.TotalHours >= 1 ? $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}"
            : $"{(int)time.TotalMinutes}:{time.Seconds:00}";
    }
}
