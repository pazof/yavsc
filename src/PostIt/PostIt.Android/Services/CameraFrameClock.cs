using System;

namespace PostIt.Android.Services;

internal sealed class CameraFrameClock
{
    private long? _offset;
    private long? _previousTimestamp;

    internal long? GetPresentationTime(long cameraTimestamp, long monotonicNow)
    {
        if (_previousTimestamp is { } previous)
        {
            if (cameraTimestamp == previous) return null;
            if (cameraTimestamp < previous)
                throw new InvalidOperationException("Les horodatages de la camera Android ont recule.");
        }
        // Camera2 may use elapsed realtime (including suspend), whereas MediaRecorder
        // and its microphone use monotonic time. Preserve frame intervals, not that epoch.
        _offset ??= monotonicNow - cameraTimestamp;
        _previousTimestamp = cameraTimestamp;
        return checked(cameraTimestamp + _offset.Value);
    }
}
