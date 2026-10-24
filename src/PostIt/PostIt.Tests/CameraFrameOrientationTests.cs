using PostIt.Android.Services;

namespace PostIt.Tests;

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

    [Theory]
    [InlineData(90, 0, -1, 0)]
    [InlineData(270, 0, 1, 0)]
    [InlineData(180, -1, 0, 0)]
    [InlineData(90, 1, 0, 90)]
    [InlineData(270, 1, 0, 270)]
    [InlineData(0, 0, -1, 270)]
    [InlineData(180, 0, -1, 90)]
    public void Does_not_repeat_rotation_already_in_surface_texture(
        int desired, float x, float y, int expected)
    {
        float[] matrix = [x, y, 0, 0, y, -x, 0, 0, 0, 0, 1, 0, 1, 1, 0, 1];
        Assert.Equal(expected, CameraFrameOrientation.GetRemainingRotation(desired, matrix));
    }

    [Fact]
    public void Recognizes_the_measured_rear_camera_transform_even_with_cropping()
    {
        float[] matrix = [0, -0.9f, 0, 0, -0.8f, 0, 0, 0, 0, 0, 1, 0, 0.9f, 0.95f, 0, 1];
        Assert.Equal(0, CameraFrameOrientation.GetRemainingRotation(90, matrix));
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
