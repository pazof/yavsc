using System.Threading.Tasks;
using Xunit;

namespace Yavsc.Org.Tests.Smoke;

/// <summary>
/// Smoke for the Blog BC. Hits <c>Yavsc.Org</c> on
/// <c>GET /BlogSpot/Index</c> — the front-end page that lists blog
/// posts. The controller is <c>BlogSpotController</c> (note the
/// capital S) under <c>Yavsc.Org/Controllers/Communicating/</c>,
/// without a class-level <c>[Route]</c>, so the conventional
/// <c>/{controller}/{action}</c> route applies (controller name
/// preserved case).
///
/// The blog backend API itself lives in <c>Yavsc.Blogs</c> on a
/// separate host in production; cf.
/// <c>doc/architecture/decoupage-organisation.md</c>. The smoke
/// here asserts the front-end side of the BC.
/// </summary>
public class BlogSmokeTests : SmokeTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public BlogSmokeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetBlogSpotIndex_returns_a_page()
    {
        using var client = _factory.CreateClient();
        await AssertResponds(client, "/BlogSpot/Index");
    }
}
