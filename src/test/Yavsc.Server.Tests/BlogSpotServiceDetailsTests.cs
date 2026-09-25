using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models.Blog;
using Yavsc.Server.Exceptions;
using Yavsc.Server.Tests.Fixtures;

namespace Yavsc.Server.Tests;

/// <summary>
/// Service-level tests for <see cref="BlogSpotService.Details"/>.
///
/// <para><see cref="BlogSpotService.Details"/> loads a
/// <see cref="BlogPost"/> (with Author, Tags, Comments, ACL and
/// Publication navigations), runs a resource-based
/// <c>ReadPermission</c> authorization against it, scrubs the ACL for
/// non-owners, derives <see cref="BlogPost.IsPublished"/> from the
/// loaded <c>Publication</c> navigation, and eagerly loads each
/// comment's <c>Author</c>. These tests pin each of those behaviours
/// at the service layer against the real
/// <see cref="Yavsc.Services.PermissionHandler"/> (the public/owner/
/// sponsor/Administrator disjunction) and a real SQLite store — no
/// mocking.</para>
///
/// <para>Seeding and the service call resolve through separate DI
/// scopes so the DbContext that writes the seed rows is disposed
/// before the one <see cref="BlogSpotService.Details"/> reads through,
/// mirroring the per-request DbContext lifetime in production.</para>
/// </summary>
[Collection("Yavsc Server")]
public sealed class BlogSpotServiceDetailsTests : IClassFixture<ServerServicesFixture>
{
    private readonly ServerServicesFixture _fixture;

    public BlogSpotServiceDetailsTests(ServerServicesFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>An authenticated principal whose <c>sub</c> claim is
    /// <paramref name="userId"/>. <see cref="Yavsc.Server.Helpers.UserHelpers.GetUserId"/>
    /// reads <c>sub</c> first, which is what the JwtBearer middleware
    /// in production preserves (MapInboundClaims = false), and what
    /// <see cref="Yavsc.Services.PermissionHandler"/> compares against
    /// <see cref="BlogPost.AuthorId"/>.</summary>
    private static ClaimsPrincipal Authenticated(string userId,
        params Claim[] extra)
    {
        var claims = new List<Claim> { new("sub", userId) };
        claims.AddRange(extra);
        return new ClaimsPrincipal(
            new ClaimsIdentity(claims, authenticationType: "Bearer"));
    }

    /// <summary>An unauthenticated principal: a
    /// <see cref="ClaimsIdentity"/> with no authentication type has
    /// <see cref="ClaimsIdentity.IsAuthenticated"/> == false. The
    /// service still runs authorization — a published post's
    /// <c>ReadPermission</c> succeeds purely on IsPublic, with no
    /// identity required.</summary>
    private static ClaimsPrincipal Anonymous()
        => new(new ClaimsIdentity());

    /// <summary>The Microsoft-format role claim
    /// <see cref="Yavsc.Server.Helpers.UserHelpers.IsInMsRole"/>
    /// reads when checking the "Administrator" bypass in
    /// <see cref="Yavsc.Services.PermissionHandler"/>.</summary>
    private static Claim AdminRole()
        => new("http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
               "Administrator");

    [Fact]
    public async Task Details_returns_the_post_for_its_owner_and_keeps_the_acl()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        var circleId = _fixture.SeedCircle("alice", "alice-private");
        var postId = _fixture.SeedBlogPost("alice", "Owner reads own draft");
        _fixture.SeedAcl(postId, circleId);

        var blog = await _fixture.WithServiceAsync(svc =>
            svc.Details(Authenticated("alice"), postId));

        Assert.Equal(postId, blog.Id);
        Assert.Equal("Owner reads own draft", blog.Title);
        // The owner is the one viewer ScrubAclForViewer keeps the ACL
        // for — a non-owner would see an empty list.
        Assert.Single(blog.ACL!);
        // No BlogSpotPublication row was seeded, so IsPublished is
        // derived from the (null) Publication navigation as false.
        Assert.False(blog.IsPublished);
    }

    [Fact]
    public async Task Details_serves_IsPublished_true_when_a_publication_row_exists()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        var postId = _fixture.SeedBlogPost("alice", "Published post");
        _fixture.Publish(postId);

        var blog = await _fixture.WithServiceAsync(svc =>
            svc.Details(Authenticated("alice"), postId));

        // IsPublished is a wire-only value the service derives from
        // the loaded Publication navigation (Details Includes it).
        Assert.True(blog.IsPublished);
    }

    [Fact]
    public async Task Details_lets_an_anonymous_viewer_read_a_published_post_and_scrubs_its_acl()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        var circleId = _fixture.SeedCircle("alice", "alice-private");
        var postId = _fixture.SeedBlogPost("alice", "Public post");
        // The ACL is real, but a non-owner viewer must not see it.
        _fixture.SeedAcl(postId, circleId);
        _fixture.Publish(postId);

        var blog = await _fixture.WithServiceAsync(svc =>
            svc.Details(Anonymous(), postId));

        // ReadPermission succeeds purely on IsPublic (the publication
        // row) — no identity needed.
        Assert.Equal(postId, blog.Id);
        Assert.True(blog.IsPublished);
        // ScrubAclForViewer: the anonymous viewer is not the owner, so
        // the ACL is replaced with an empty list rather than leaked.
        Assert.Empty(blog.ACL!);
    }

    [Fact]
    public async Task Details_denies_a_draft_to_a_non_owner_who_is_not_a_sponsor()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        _fixture.SeedUser("bob");
        var postId = _fixture.SeedBlogPost("alice", "Alice's draft");

        // No publication row (not public), bob is not the owner, bob
        // is not a member of any circle alice owns, bob is not an
        // Administrator — every branch of ReadPermission fails.
        await Assert.ThrowsAsync<AuthorizationFailureException>(() =>
            _fixture.WithServiceAsync(svc =>
                svc.Details(Authenticated("bob"), postId)));
    }

    [Fact]
    public async Task Details_lets_a_sponsor_read_a_draft_and_scrubs_its_acl()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        _fixture.SeedUser("bob");
        // Sponsorship = bob is a member of a circle alice owns.
        var circleId = _fixture.SeedCircle("alice", "alice-friends");
        _fixture.SeedCircleMember(circleId, "bob");
        var postId = _fixture.SeedBlogPost("alice", "Draft for friends");
        _fixture.SeedAcl(postId, circleId);

        var blog = await _fixture.WithServiceAsync(svc =>
            svc.Details(Authenticated("bob"), postId));

        // ReadPermission succeeds on the IsSponsor branch.
        Assert.Equal(postId, blog.Id);
        Assert.False(blog.IsPublished);
        // bob is not the owner, so the ACL is scrubbed even though he
        // was granted read access via the circle.
        Assert.Empty(blog.ACL!);
    }

    [Fact]
    public async Task Details_lets_an_administrator_read_a_draft()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        _fixture.SeedUser("root");
        var postId = _fixture.SeedBlogPost("alice", "Draft, admin view");

        var blog = await _fixture.WithServiceAsync(svc =>
            svc.Details(Authenticated("root", AdminRole()), postId));

        // The Administrator bypass in PermissionHandler short-circuits
        // the public/owner/sponsor checks.
        Assert.Equal(postId, blog.Id);
        Assert.False(blog.IsPublished);
    }

    [Fact]
    public async Task Details_loads_each_comment_author()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        _fixture.SeedUser("bob");
        var postId = _fixture.SeedBlogPost("alice", "Post with comments");
        _fixture.SeedComment(postId, "bob", "Nice post!");

        var blog = await _fixture.WithServiceAsync(svc =>
            svc.Details(Authenticated("alice"), postId));

        var comment = Assert.Single(blog.Comments);
        // Details explicitly loads c.Author from _context.Users for
        // every comment; the navigation must not be left null.
        Assert.NotNull(comment.Author);
        Assert.Equal("bob", comment.Author.UserName);
    }

    [Fact]
    public async Task Details_throws_for_a_missing_post()
    {
        _fixture.ResetDatabase();

        // The query uses SingleAsync, which throws
        // InvalidOperationException when no row matches — the
        // `if (blog == null) return null` guard below it is unreachable.
        // Pin that current behaviour so a future switch to
        // SingleOrDefaultAsync (which would make the guard live) is a
        // deliberate, reviewed change rather than a silent one.
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _fixture.WithServiceAsync(svc =>
                svc.Details(Authenticated("alice"), 424242)));
    }
}