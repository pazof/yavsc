namespace Yavsc.Org.Tests.NonRegression;

/// <summary>
/// Non-regression: avatar requests under /avatars must never return 404
/// for missing files. The pipeline falls back to static defaults under
/// /images/Users/icon_user*.png.
/// </summary>
public class AvatarFallbackTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AvatarFallbackTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/avatars/user-does-not-exist.png")]
    [InlineData("/avatars/user-does-not-exist.s.png")]
    [InlineData("/avatars/user-does-not-exist.xs.png")]
    public async Task Missing_avatar_file_returns_default_image_instead_of_404(string path)
    {
        using var client = _factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;

        var response = await client.GetAsync(path, ct);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);

        var payload = await response.Content.ReadAsByteArrayAsync(ct);
        Assert.True(payload.Length > 0, $"Expected a non-empty fallback image for {path}.");
    }
}
