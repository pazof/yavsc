using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Controllers;
using Yavsc.Models;
using Yavsc.Models.Blog;

namespace Yavsc.Org.Tests.Controllers;

public class CommentsControllerTests
{
    [Fact]
    public async Task Create_sets_author_and_persists_comment()
    {
        var dbName = $"comments-controller-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var db = new ApplicationDbContext(options);
        var post = new BlogPost
        {
            Title = "Post de test",
            AuthorId = "post-author",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };
        db.BlogSpot.Add(post);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var controller = new CommentsController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, "comment-author")
                    ], "TestAuth"))
                }
            }
        };

        var comment = new Comment
        {
            ReceiverId = post.Id,
            Article = "Commentaire de test",
            Visible = true
        };

        var result = await controller.Create(comment);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var stored = await db.Comment.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal("comment-author", stored.AuthorId);
        Assert.Equal(post.Id, stored.ReceiverId);
        Assert.Equal("Commentaire de test", stored.Article);
    }
}
