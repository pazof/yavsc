using System;

namespace PostIt.Services;

public sealed class ReplayControlsState(TimeProvider? clock = null)
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;
    private DateTimeOffset _lastInteraction = (clock ?? TimeProvider.System).GetUtcNow();
    public bool IsInteracting { get; set; }
    public void Show() => _lastInteraction = _clock.GetUtcNow();
    public bool IsVisible(bool playing, bool focused)
        => !playing || focused || IsInteracting || _clock.GetUtcNow() - _lastInteraction < TimeSpan.FromSeconds(3);

    public static string FormatTime(long milliseconds)
    {
        var time = TimeSpan.FromMilliseconds(Math.Max(0, milliseconds));
        return time.TotalHours >= 1 ? $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}"
            : $"{(int)time.TotalMinutes}:{time.Seconds:00}";
    }
}
