using PostIt.Views.Commands;

namespace PostIt.Tests;

public class RdvPageTests
{
    [Fact]
    public void AreCoordinatesClose_returns_true_for_nearby_points_within_cache_tolerance()
    {
        var result = RdvPage.AreCoordinatesClose(
            48.8566,
            2.3522,
            48.85665,
            2.35225);

        Assert.True(result);
    }

    [Fact]
    public void AreCoordinatesClose_returns_false_for_points_outside_cache_tolerance()
    {
        var result = RdvPage.AreCoordinatesClose(
            48.8566,
            2.3522,
            48.85685,
            2.3522);

        Assert.False(result);
    }
}