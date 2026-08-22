using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Models;
using Yavsc.Models.Relationship;
using Yavsc.Tests.Shared;
using static Yavsc.Constants;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Behavioural tests for the circle-members endpoints on
/// <c>CircleApiController</c>:
/// <c>GET /api/circle/{id}/members</c>,
/// <c>POST /api/circle/{id}/members</c>,
/// <c>DELETE /api/circle/{id}/members/{userId}</c>.
///
/// <para>Same fixture as <see cref="BlogApiTests"/>:
/// <see cref="BlogsWebServerFixture"/> provides an in-memory
/// <c>ApplicationDbContext</c>, JWT bearer auth with HS256,
/// and the production <c>BlogScope</c> policy. Tests use
/// <c>TestTokenIssuer</c> to mint tokens whose <c>sub</c>
/// claim identifies the caller.</para>
///
/// <para>Test users (<c>alice</c>, <c>bob</c>) are seeded
/// directly via <see cref="ApplicationDbContext.Users"/>:
/// the Blogs fixture doesn't stand up
/// <c>UserManager&lt;ApplicationUser&gt;</c>, so we go
/// through the DbContext the same way the production code
/// would.</para>
/// </summary>
[Collection("JwtClaimMapping")]
public sealed class CircleMembersApiTests : IClassFixture<BlogsWebServerFixture>
{
    private readonly BlogsWebServerFixture _fixture;

    public CircleMembersApiTests(BlogsWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>Reset the in-memory database and seed
    /// <c>alice</c> + <c>bob</c>. <c>UseInMemoryDatabase</c>
    /// shares its store across the fixture lifetime, so each
    /// test starts from a clean slate.</summary>
    private void ResetDatabaseWithUsers()
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
            FullName = "Alice Dupont",
            Avatar = "/avatars/alice.png",
        });
        db.Users.Add(new ApplicationUser
        {
            Id = "bob",
            UserName = "bob",
            Email = "bob@example.com",
            EmailConfirmed = true,
            FullName = "Bob Martin",
            Avatar = "/avatars/bob.png",
        });
        db.SaveChanges();
    }

    /// <summary>Create a circle owned by <paramref name="ownerId"/>
    /// directly in the in-memory store and return its server-assigned
    /// id. The tests below use this to bypass the controller's POST
    /// (which is already covered by other tests on the branch);
    /// the focus here is the members endpoints.</summary>
    private long SeedCircle(string ownerId, string name)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var circle = new Circle { OwnerId = ownerId, Name = name };
        db.Circle.Add(circle);
        db.SaveChanges();
        return circle.Id;
    }

    private string MembersUrl(long circleId)
        => $"{_fixture.Addresses.First(a => a.StartsWith("https://"))}/{APIPrefix}/circle/{circleId}/members";

    private HttpClient NewClient(string subject)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(_fixture.Addresses.First(a => a.StartsWith("https://")))
        };
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", TestTokenIssuer.Issue(subject));
        return http;
    }

    [Fact]
    public async Task GetMembers_returns_200_with_empty_list_when_no_members()
    {
        ResetDatabaseWithUsers();
        var circleId = SeedCircle("alice", "Famille");
        using var http = NewClient("alice");

        var response = await http.GetAsync(MembersUrl(circleId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task PostMember_returns_201_then_Get_returns_the_member()
    {
        ResetDatabaseWithUsers();
        var circleId = SeedCircle("alice", "Famille");
        using var http = NewClient("alice");

        var postResponse = await http.PostAsJsonAsync(
            MembersUrl(circleId),
            new { userId = "bob" });

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var getResponse = await http.GetAsync(MembersUrl(circleId));
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var doc = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(1, doc.RootElement.GetArrayLength());
        var member = doc.RootElement[0];
        Assert.Equal("bob", member.GetProperty("id").GetString());
        Assert.Equal("bob", member.GetProperty("userName").GetString());
        Assert.Equal("Bob Martin", member.GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task PostMember_returns_409_when_user_already_in_circle()
    {
        ResetDatabaseWithUsers();
        var circleId = SeedCircle("alice", "Famille");
        using var http = NewClient("alice");

        var first = await http.PostAsJsonAsync(
            MembersUrl(circleId),
            new { userId = "bob" });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await http.PostAsJsonAsync(
            MembersUrl(circleId),
            new { userId = "bob" });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task DeleteMember_returns_200_then_Get_does_not_include_member()
    {
        ResetDatabaseWithUsers();
        var circleId = SeedCircle("alice", "Famille");
        using var http = NewClient("alice");

        await http.PostAsJsonAsync(MembersUrl(circleId), new { userId = "bob" });

        var deleteResponse = await http.DeleteAsync(
            $"{MembersUrl(circleId)}/bob");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var getResponse = await http.GetAsync(MembersUrl(circleId));
        using var doc = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task GetMembers_returns_404_when_circle_not_owned_by_caller()
    {
        ResetDatabaseWithUsers();
        // Alice's circle, Bob tries to read its members.
        var circleId = SeedCircle("alice", "Famille");
        using var http = NewClient("bob");

        var response = await http.GetAsync(MembersUrl(circleId));

        // 404, not 403 — the controller deliberately avoids leaking
        // the existence of someone else's circle.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
