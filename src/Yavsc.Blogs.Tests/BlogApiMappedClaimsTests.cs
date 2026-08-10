using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Yavsc.Models;
using Yavsc.Models.Blog;
using Yavsc.Tests.Shared;

namespace Yavsc.Blogs.Tests;

[Collection("JwtClaimMapping")]
public sealed class BlogApiMappedClaimsTests : IClassFixture<MappedClaimsBlogsWebServerFixture>
{
    private readonly MappedClaimsBlogsWebServerFixture _fixture;

    public BlogApiMappedClaimsTests(MappedClaimsBlogsWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    private void ResetDatabase()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    private HttpClient NewClient(string subject = "tester")
    {
        var http = new HttpClient
        {
            BaseAddress = new Uri(_fixture.Addresses.First())
        };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IssueMappedClaimsToken(subject));
        return http;
    }

    private static string IssueMappedClaimsToken(string subject)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new("sub", subject),
            new("scope", "blogs"),
        };

        var token = new JwtSecurityToken(
            issuer: TestTokenIssuer.Issuer,
            audience: TestTokenIssuer.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddHours(1),
            signingCredentials: new SigningCredentials(
                TestTokenIssuer.SigningKey,
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task PostBlog_with_mapped_sub_claim_sets_AuthorId_from_authenticated_user()
    {
        ResetDatabase();
        using var http = NewClient(subject: "mapped-user");

        var draft = new BlogPost
        {
            Id = 0,
            Title = "Billet JWT remappe",
            AuthorId = "payload-attacker",
            Article = "Contenu de test.",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };

        var response = await http.PostAsJsonAsync("/api/v1/blog", draft);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<BlogPost>();
        Assert.NotNull(created);
        Assert.Equal("mapped-user", created!.AuthorId);
    }

    [Fact]
    public async Task PutBlog_with_mapped_sub_claim_allows_owner_to_update()
    {
        ResetDatabase();
        using var http = NewClient(subject: "mapped-owner");

        var createdResponse = await http.PostAsJsonAsync("/api/v1/blog", new BlogPost
        {
            Id = 0,
            Title = "Billet à modifier",
            AuthorId = "payload-attacker",
            Article = "Contenu initial.",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        });

        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var created = await createdResponse.Content.ReadFromJsonAsync<BlogPost>();
        Assert.NotNull(created);

        var updateResponse = await http.PutAsJsonAsync($"/api/v1/blog/{created!.Id}", new BlogPost
        {
            Id = created.Id,
            Title = "Billet modifié",
            AuthorId = created.AuthorId,
            Article = "Contenu mis à jour.",
            DateCreated = created.DateCreated,
            DateModified = DateTime.UtcNow
        });

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
    }

    [Fact]
    public async Task PutBlog_with_mapped_sub_claim_rejects_non_owner()
    {
        ResetDatabase();
        using var ownerHttp = NewClient(subject: "mapped-owner");

        var createdResponse = await ownerHttp.PostAsJsonAsync("/api/v1/blog", new BlogPost
        {
            Id = 0,
            Title = "Billet protégé",
            AuthorId = "payload-attacker",
            Article = "Contenu initial.",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        });

        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var created = await createdResponse.Content.ReadFromJsonAsync<BlogPost>();
        Assert.NotNull(created);

        using var otherHttp = NewClient(subject: "mapped-other");
        var updateResponse = await otherHttp.PutAsJsonAsync($"/api/v1/blog/{created!.Id}", new BlogPost
        {
            Id = created.Id,
            Title = "Tentative de modification",
            AuthorId = created.AuthorId,
            Article = "Contenu non autorisé.",
            DateCreated = created.DateCreated,
            DateModified = DateTime.UtcNow
        });

        Assert.Equal(HttpStatusCode.Unauthorized, updateResponse.StatusCode);
    }
}
