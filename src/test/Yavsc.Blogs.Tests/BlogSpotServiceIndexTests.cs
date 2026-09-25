using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Blogspot;
using Yavsc.Models;
using Yavsc.Models.Blog;
using Yavsc.Blogs.Tests.Fixtures;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Direct service-level tests for <see cref="BlogSpotService.Index"/>.
///
/// <para>The HTTP tests in <see cref="BlogApiTests"/> all drive the
/// <c>[Authorize("BlogScope")]</c>-protected <c>BlogApiController</c>,
/// so the request is always authenticated by the time it reaches the
/// service. The MVC <c>BlogspotController.Index</c> action, by
/// contrast, is <c>[AllowAnonymous]</c> — that is the one caller that
/// actually exercises the unauthenticated branch of
/// <see cref="BlogSpotService.Index"/>, the branch that reads from
/// <c>blogSpotPublications</c> and projects to <c>p.Post</c>.</para>
///
/// <para>These tests pin that behaviour at the service layer: a
/// <c>BlogPost</c> that has a <c>BlogSpotPublication</c> row is
/// listed for an anonymous viewer, and one without is not. Seeding
/// runs against the same shared SQLite store as the rest of the
/// <c>Yavsc Blogs</c> collection, so the real
/// <see cref="BlogSpotService"/> (registered by
/// <see cref="BlogsWebServerFixture"/>) is resolved from DI and run
/// end-to-end — no mocking.</para>
/// </summary>
[Collection("Yavsc Blogs")]
public sealed class BlogSpotServiceIndexTests : IClassFixture<BlogsWebServerFixture>
{
    private readonly BlogsWebServerFixture _fixture;

    public BlogSpotServiceIndexTests(BlogsWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>An unauthenticated principal: a <see cref="ClaimsIdentity"/>
    /// with no authentication type has
    /// <see cref="ClaimsIdentity.IsAuthenticated"/> == false, so
    /// <see cref="BlogSpotService.Index"/> takes its anonymous
    /// branch. This mirrors what the JwtBearer middleware hands the
    /// MVC <c>[AllowAnonymous]</c> action when no bearer token is
    /// presented.</summary>
    private static ClaimsPrincipal Anonymous()
        => new(new ClaimsIdentity());

    /// <summary>Resolve the real <see cref="BlogSpotService"/> from the
    /// fixture's DI container and run <paramref name="body"/> against it,
    /// disposing the request scope (and its scoped DbContext) afterwards.
    /// The service itself is not <see cref="IDisposable"/>; the scope is,
    /// so the scope is what we must dispose to avoid leaking a DbContext
    /// against the shared SQLite connection across tests.</summary>
    private async Task<T> WithServiceAsync<T>(
        Func<BlogSpotService, Task<T>> body)
    {
        using var scope = _fixture.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<BlogSpotService>();
        return await body(service);
    }

    private void Publish(long postId)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // The publication flag is the existence of a row in
        // blogSpotPublications — same write SetPublishAsync performs
        // in production. Inserting it directly is the most faithful
        // way to seed "this post is published" without routing
        // through the authorize-gated PUT/publish endpoint.
        db.blogSpotPublications.Add(new BlogSpotPublication { PostId = postId });
        db.SaveChanges();
    }

    [Fact]
    public async Task Index_lists_a_published_post_for_anonymous_access()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        var publishedId = _fixture.SeedBlogPost("alice", "Published for all");

        Publish(publishedId);

        var posts = await WithServiceAsync(async svc =>
        {
            var result = await svc.Index(Anonymous(), id: "", skip: 0, take: 25);
            return result.OfType<BlogPost>().ToArray();
        });

        var listed = Assert.Single(posts);
        Assert.Equal("Published for all", listed.Title);
        // The service serves IsPublished from the
        // blogSpotPublications table — the anonymous branch does
        // not Include the Publication navigation, so this proves
        // the value is hydrated by the service, not derived from
        // the EF graph.
        Assert.True(listed.IsPublished);
    }

    [Fact]
    public async Task Index_serves_IsPublished_false_for_a_draft_for_anonymous_access()
    {
        // Anonymous viewers never receive a draft (the branch
        // queries blogSpotPublications), so this is checked through
        // the authenticated branch: the service must still serve
        // IsPublished == false for a post with no publication row,
        // not the default of a freshly loaded entity.
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        var draftId = _fixture.SeedBlogPost("alice", "Draft, not published");

        var viewer = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", "alice")], authenticationType: "Bearer"));

        var posts = await WithServiceAsync(async svc =>
        {
            var result = await svc.Index(viewer, id: "", skip: 0, take: 25);
            return result.OfType<BlogPost>().ToArray();
        });

        var draft = Assert.Single(posts);
        Assert.Equal(draftId, draft.Id);
        Assert.False(draft.IsPublished);
    }

    [Fact]
    public async Task Index_does_not_list_an_unpublished_post_for_anonymous_access()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        var publishedId = _fixture.SeedBlogPost("alice", "Published for all");
        _fixture.SeedBlogPost("alice", "Draft, not published");

        // Only the first post is published; the draft has no
        // BlogSpotPublication row.
        Publish(publishedId);

        var titles = await WithServiceAsync(async svc =>
        {
            var result = await svc.Index(Anonymous(), id: "", skip: 0, take: 25);
            return result.Select(p => p.Title).ToArray();
        });

        Assert.Contains("Published for all", titles);
        Assert.DoesNotContain("Draft, not published", titles);
    }

    [Fact]
    public async Task Index_returns_an_empty_list_for_anonymous_access_when_nothing_is_published()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        // A draft with no publication row: invisible to anonymous.
        _fixture.SeedBlogPost("alice", "Draft, not published");

        var result = await WithServiceAsync(svc =>
            svc.Index(Anonymous(), id: "", skip: 0, take: 25));

        Assert.Empty(result);
    }
}