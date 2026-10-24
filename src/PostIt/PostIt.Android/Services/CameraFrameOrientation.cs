using System;

namespace PostIt.Android.Services;

internal static class CameraFrameOrientation
{
    internal static int GetRotation(int sensorDegrees, int displayDegrees, bool frontFacing)
    {
        Validate(sensorDegrees);
        Validate(displayDegrees);
        return (sensorDegrees + (frontFacing ? displayDegrees : -displayDegrees) + 360) % 360;
    }

    internal static float[] GetTextureCoordinates(int clockwiseDegrees)
    {
        Validate(clockwiseDegrees);
        float[] corners = [0, 0, 1, 0, 0, 1, 1, 1];
        for (var i = 0; i < corners.Length; i += 2)
        {
            var x = corners[i];
            var y = corners[i + 1];
            (corners[i], corners[i + 1]) = clockwiseDegrees switch
            {
                90 => (1 - y, x),
                180 => (1 - x, 1 - y),
                270 => (y, 1 - x),
                _ => (x, y)
            };
        }
        return corners;
    }

    internal static int GetRemainingRotation(int clockwiseDegrees, float[] textureMatrix)
    {
        Validate(clockwiseDegrees);
        if (textureMatrix.Length != 16)
            throw new ArgumentException("A SurfaceTexture transform must contain 16 elements.", nameof(textureMatrix));
        // SurfaceTexture's usual Y flip changes the sign of the first column's Y.
        // Camera2 can also put the sensor rotation in this matrix; don't apply it twice.
        var angle = Math.Atan2(-textureMatrix[1], textureMatrix[0]) * 180 / Math.PI;
        if (!double.IsFinite(angle) || (textureMatrix[0] == 0 && textureMatrix[1] == 0))
            throw new ArgumentException("Invalid SurfaceTexture orientation.", nameof(textureMatrix));
        var quarterTurns = (int)Math.Round(angle / 90);
        if (Math.Abs(angle - quarterTurns * 90) > 0.01)
            throw new ArgumentException("SurfaceTexture orientation is not a quarter turn.", nameof(textureMatrix));
        var producerRotation = (quarterTurns * 90 + 360) % 360;
        return (clockwiseDegrees - producerRotation + 360) % 360;
    }

    private static void Validate(int degrees)
    {
        if (degrees is not (0 or 90 or 180 or 270))
            throw new ArgumentOutOfRangeException(nameof(degrees), degrees, "Rotation must be 0, 90, 180 or 270 degrees.");
    }
}
