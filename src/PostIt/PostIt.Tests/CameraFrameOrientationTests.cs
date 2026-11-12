using PostIt.Android.Services;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
public sealed class CameraFrameOrientationTests
{
    [Theory]
    [InlineData(90, 0, false, 90)]
    [InlineData(90, 90, false, 0)]
    [InlineData(90, 180, false, 270)]
    [InlineData(90, 270, false, 180)]
    [InlineData(270, 0, true, 270)]
    [InlineData(270, 90, true, 0)]
    [InlineData(270, 180, true, 90)]
    [InlineData(270, 270, true, 180)]
    [InlineData(0, 90, false, 270)]
    [InlineData(180, 270, true, 90)]
    public void Uses_sensor_display_and_lens_facing(int sensor, int display, bool front, int expected)
        => Assert.Equal(expected, CameraFrameOrientation.GetRotation(sensor, display, front));

    [Theory]
    [InlineData(0, new float[] { 0, 0, 1, 0, 0, 1, 1, 1 })]
    [InlineData(90, new float[] { 1, 0, 1, 1, 0, 0, 0, 1 })]
    [InlineData(180, new float[] { 1, 1, 0, 1, 1, 0, 0, 0 })]
    [InlineData(270, new float[] { 0, 1, 0, 0, 1, 1, 1, 0 })]
    public void Maps_output_corners_to_input_for_clockwise_rotation(int rotation, float[] expected)
        => Assert.Equal(expected, CameraFrameOrientation.GetTextureCoordinates(rotation));

    [Fact]
    public void Sensor_rotation_and_display_changes_map_corners_for_all_lens_orientations()
    {
        foreach (var sensor in new[] { 0, 90, 180, 270 })
        foreach (var display in new[] { 0, 90, 180, 270 })
        foreach (var front in new[] { false, true })
        {
            var rotation = CameraFrameOrientation.GetRotation(sensor, display, front);
            var coordinates = CameraFrameOrientation.GetTextureCoordinates(rotation);
            float[] expected = [0, 0, 1, 0, 0, 1, 1, 1];
            var turns = (sensor / 90 + (front ? display / 90 : -display / 90) + 4) % 4;
            for (var turn = 0; turn < turns; turn++)
                for (var index = 0; index < expected.Length; index += 2)
                    (expected[index], expected[index + 1]) = (1 - expected[index + 1], expected[index]);
            Assert.Equal(expected, coordinates);
        }
    }

    [Theory]
    [InlineData(new float[] { 1, 0, 0, 0, 0, -1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1 }, 90, 90)]
    [InlineData(new float[] { 1, 0, 0, 0, 0, -1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1 }, 270, 270)]
    // Measured on a real Android 15 phone in portrait (rear sensor 90°, front sensor 270°).
    [InlineData(new float[] { 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 1 }, 90, 0)]
    [InlineData(new float[] { 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 1 }, 0, 270)]
    [InlineData(new float[] { 0, -1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1 }, 270, 0)]
    [InlineData(new float[] { 0, -1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1 }, 0, 270)]
    [InlineData(new float[] { 0, -1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1 }, 180, 90)]
    public void Does_not_apply_the_rotation_already_in_the_texture_transform_twice(
        float[] matrix, int rotation, int expected)
        => Assert.Equal(expected, CameraFrameOrientation.GetRemainingRotation(rotation, matrix));

    [Fact]
    public void Rejects_invalid_texture_transforms()
    {
        Assert.Throws<ArgumentException>(() => CameraFrameOrientation.GetRemainingRotation(0, new float[15]));
        Assert.Throws<ArgumentException>(() => CameraFrameOrientation.GetRemainingRotation(0, new float[16]));
        var skewed = new float[] { 0.7f, -0.7f, 0, 0, -0.7f, -0.7f, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1 };
        Assert.Throws<ArgumentException>(() => CameraFrameOrientation.GetRemainingRotation(0, skewed));
    }

    [Theory]
    [InlineData(-90)]
    [InlineData(45)]
    [InlineData(360)]
    public void Rejects_invalid_angles(int rotation)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CameraFrameOrientation.GetRotation(rotation, 0, false));
        Assert.Throws<ArgumentOutOfRangeException>(() => CameraFrameOrientation.GetRotation(90, rotation, false));
        Assert.Throws<ArgumentOutOfRangeException>(() => CameraFrameOrientation.GetTextureCoordinates(rotation));
    }
}
