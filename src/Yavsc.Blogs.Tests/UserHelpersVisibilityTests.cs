using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Blogspot;
using Yavsc.Models;
using Yavsc.Models.Access;
using Yavsc.Models.Blog;
using Yavsc.Models.Relationship;
using Yavsc.Server.Helpers;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Tests that <see cref="UserHelpers.UserPosts"/> (the
/// "posts-by-author-for-this-reader" query) honours the same
/// Visibility rules as <see cref="BlogSpotService.Index"/>.
///
/// <para>The two code paths duplicate the ACL/Visibility filter
/// (one in the listing query, one in the per-author query);
/// these tests catch the case where the two diverge — the kind
/// of regression that's easy to miss in a code review because
/// both filters look correct in isolation.</para>
///
/// <para>Uses the same in-memory <c>ApplicationDbContext</c>
/// scaffold as <see cref="BlogsWebServerFixture"/> but
/// exercises the helper directly, without going through HTTP,
/// because <see cref="UserHelpers.UserPosts"/> is the unit
/// under test.</para>
/// </summary>
[Collection("JwtClaimMapping")]
public sealed class UserHelpersVisibilityTests : IClassFixture<BlogsWebServerFixture>
{
    private readonly BlogsWebServerFixture _fixture;

    public UserHelpersVisibilityTests(BlogsWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    private void ResetDatabase()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        db.Users.Add(new ApplicationUser
        {
            Id = "alice",
            UserName = "alice",
            Email = "alice@example.com",
            EmailConfirmed = true,
        });
        db.Users.Add(new ApplicationUser
        {
            Id = "bob",
            UserName = "bob",
            Email = "bob@example.com",
            EmailConfirmed = true,
        });
        db.SaveChanges();
    }

    private long SeedPost(string authorId, Visibility visibility, params long[] aclCircleIds)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var post = new BlogPost
        {
            AuthorId = authorId,
            Title = $"post-by-{authorId}",
            Article = "test article",
            Visibility = visibility,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };
        db.BlogSpot.Add(post);
        db.SaveChanges();
        foreach (var cid in aclCircleIds)
        {
            db.CircleAuthorizationToBlogPost.Add(new CircleAuthorizationToBlogPost
            {
                BlogPostId = post.Id,
                CircleId = cid,
                Comment = false,
            });
        }
        db.SaveChanges();
        return post.Id;
    }

    private long SeedCircle(string ownerId, string name)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var circle = new Circle { OwnerId = ownerId, Name = name };
        db.Circle.Add(circle);
        db.SaveChanges();
        return circle.Id;
    }

    private void AddMember(long circleId, string memberId)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.CircleMembers.Add(new CircleMember { CircleId = circleId, MemberId = memberId });
        db.SaveChanges();
    }

    private List<long> UserPostsIds(string posterId, string readerId)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return db.UserPosts(posterId, readerId).Select(p => p.Id).ToList();
    }

    [Fact]
    public void UserPosts_returns_only_private_posts_to_their_author()
    {
        ResetDatabase();
        SeedPost("alice", Visibility.Private);

        var aliceSees = UserPostsIds("alice", "alice");
        var bobSees = UserPostsIds("alice", "bob");

        Assert.Single(aliceSees);
        Assert.Empty(bobSees);
    }

    [Fact]
    public void UserPosts_returns_public_posts_with_empty_ACL_to_anyone()
    {
        ResetDatabase();
        SeedPost("alice", Visibility.Public);

        var bobSees = UserPostsIds("alice", "bob");
        Assert.Single(bobSees);
    }

    [Fact]
    public void UserPosts_narrows_public_posts_with_nonempty_ACL()
    {
        ResetDatabase();
        var circleId = SeedCircle("alice", "Famille");
        AddMember(circleId, "alice");
        // AddMember above adds alice, but we want bob NOT in
        // the circle, so we add bob to a different circle only:
        var otherCircleId = SeedCircle("alice", "Travail");
        AddMember(otherCircleId, "bob");
        // Make the post readable only to Famille:
        SeedPost("alice", Visibility.Public, circleId);

        var bobSees = UserPostsIds("alice", "bob");
        Assert.Empty(bobSees);
    }

    [Fact]
    public void UserPosts_lets_acl_members_read_public_posts_even_if_not_author()
    {
        ResetDatabase();
        var circleId = SeedCircle("alice", "Famille");
        AddMember(circleId, "bob");
        SeedPost("alice", Visibility.Public, circleId);

        var bobSees = UserPostsIds("alice", "bob");
        Assert.Single(bobSees);
    }
}
