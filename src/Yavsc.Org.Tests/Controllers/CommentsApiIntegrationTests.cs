using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Models;
using Yavsc.Models.Blog;
using Yavsc.Tests.Shared;

namespace Yavsc.Org.Tests.Controllers;

public class CommentsApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public CommentsApiIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_blogcomments_json_returns_201_and_persists_comment()
    {
        long postId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!db.Users.Any(u => u.Id == TestUserMiddleware.UserId))
            {
                db.Users.Add(new ApplicationUser
                {
                    Id = TestUserMiddleware.UserId,
                    UserName = "test-user",
                    NormalizedUserName = "TEST-USER",
                    Email = "test-user@example.com",
                    NormalizedEmail = "TEST-USER@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("N"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("N")
                });
            }

            var post = new BlogPost
            {
                Title = "Post for comment API test",
                AuthorId = TestUserMiddleware.UserId,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            db.BlogSpot.Add(post);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            postId = post.Id;
        }

        var http = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
        http.DefaultRequestHeaders.Add(TestAuthPolicyProvider.HeaderName, TestAuthPolicyProvider.AdminRole);

        var response = await http.PostAsJsonAsync(
            "/api/v1/blogcomments",
            new
            {
                Article = "Comment API integration test",
                ReceiverId = postId
            },
            TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(
            response.StatusCode != HttpStatusCode.InternalServerError,
            $"Unexpected 500 on POST /api/v1/blogcomments. Body: {responseBody}");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains("\"id\"", responseBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"dateCreated\"", responseBody, StringComparison.OrdinalIgnoreCase);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = await verifyDb.Comment
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(c => c.ReceiverId == postId, TestContext.Current.CancellationToken);

        Assert.NotNull(stored);
        Assert.Equal("Comment API integration test", stored!.Article);
        Assert.Equal(TestUserMiddleware.UserId, stored.AuthorId);
    }
}
