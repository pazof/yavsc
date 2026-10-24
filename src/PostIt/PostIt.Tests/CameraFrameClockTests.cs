using PostIt.Android.Services;

namespace PostIt.Tests;

public sealed class CameraFrameClockTests
{
    [Theory]
    [InlineData(4_213_000_000_000L)]
    [InlineData(0L)]
    [InlineData(-1_000_000_000L)]
    public void Aligns_camera_epoch_to_encoder_and_preserves_frame_intervals(long cameraOffset)
    {
        var clock = new CameraFrameClock();
        const long now = 20_000_000_000_000L;
        var camera = now + cameraOffset;
        Assert.Equal(now, clock.GetPresentationTime(camera, now));
        Assert.Equal(now + 33_333_333, clock.GetPresentationTime(camera + 33_333_333, now + 40_000_000));
        Assert.Equal(now + 100_000_000, clock.GetPresentationTime(camera + 100_000_000, now + 110_000_000));
    }

    [Fact]
    public void Does_not_submit_the_same_camera_image_twice()
    {
        var clock = new CameraFrameClock();
        Assert.Equal(100, clock.GetPresentationTime(1000, 100));
        Assert.Null(clock.GetPresentationTime(1000, 110));
        Assert.Equal(120, clock.GetPresentationTime(1020, 130));
    }

    [Fact]
    public void Reports_a_camera_clock_that_goes_backwards()
    {
        var clock = new CameraFrameClock();
        clock.GetPresentationTime(1000, 100);
        Assert.Throws<InvalidOperationException>(() => clock.GetPresentationTime(999, 110));
    }
}
