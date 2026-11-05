using PostIt.Services;

namespace PostIt.Tests;

public sealed class ScreenAwakeRequestsTests
{
    [Fact]
    public void Independent_requests_keep_the_screen_awake_until_all_are_released()
    {
        var requests = new ScreenAwakeRequests();
        var states = new List<bool>();
        requests.Changed += () => states.Add(requests.IsRequested);
        Assert.False(requests.IsRequested);
        var playback = requests.Acquire();
        var capture = requests.Acquire();
        playback.Dispose();
        Assert.True(requests.IsRequested);
        playback.Dispose();
        capture.Dispose();
        Assert.False(requests.IsRequested);
        Assert.Equal(new[] { true, true, true, false }, states);
    }
}
