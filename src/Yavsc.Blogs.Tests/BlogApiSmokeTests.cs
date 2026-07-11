using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Tests.Shared;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Smoke tests for the Yavsc.Blogs API host. These tests only assert
/// that the fixture boots and the test HTTP client reaches the
/// controller pipeline — they do not yet exercise the controller
/// surface. The first behavioural test (GET /api/v1/blog returns
/// 200) lands in a follow-up commit.
/// </summary>
public sealed class BlogApiSmokeTests : IClassFixture<BlogsWebServerFixture>
{
    private readonly BlogsWebServerFixture _fixture;

    public BlogApiSmokeTests(BlogsWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Fixture_Binds_At_Least_One_Https_Address()
    {
        Assert.NotEmpty(_fixture.Addresses);
        Assert.Contains(_fixture.Addresses, a => a.StartsWith("https://"));
    }

    [Fact]
    public void Fixture_Exposes_Resolving_ServiceProvider()
    {
        // If the host built correctly, the service provider should
        // be available and resolvable. We don't need to assert a
        // specific service here — the GET 200 test will exercise
        // the BlogSpotService indirectly.
        Assert.NotNull(_fixture.Services);
        using var scope = _fixture.Services.CreateScope();
        Assert.NotNull(scope.ServiceProvider);
    }
}
