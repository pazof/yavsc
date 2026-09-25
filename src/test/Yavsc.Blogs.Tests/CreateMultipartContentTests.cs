using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Api.Client;
using Yavsc.Models;
using Yavsc.Models.Blog;
using Yavsc.Tests.Shared;
using Yavsc.Blogs.Tests.Fixtures;
using BlogPostDto = Yavsc.Blogspot.BlogPostDto;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Direct tests for <see cref="BlogApiClient.CreateMultipartContent"/>
/// — the body builder PostIt runs when a post is saved with
/// attachments. Two layers:
///
/// <list type="number">
///   <item><description><b>Shape</b>: call the method with a sample
///   <see cref="BlogPostDto"/> + a couple of
///   <see cref="BlogUploadFile"/>s and assert the produced
///   <see cref="MultipartFormDataContent"/> carries a <c>blog</c>
///   part (camelCase JSON, <c>WhenWritingNull</c> so unset graph
///   members are dropped) and one <c>file</c> part per attachment
///   (right filename, right bytes, declared Content-Type, and the
///   <c>application/octet-stream</c> fallback when the type is
///   unset). This is the contract the controller's
///   <c>ReadBlogRequestAsync</c> + <c>AttachFiles</c> rely on; if
///   the wire shape drifts, the round-trip below fails first, but
///   the shape assertions pin down <i>which</i> field broke.</description></item>
///   <item><description><b>Round-trip</b>: POST the very content the
///   method built to the live <see cref="BlogsWebServerFixture"/>
///   host (authenticated as <c>tester</c>) and assert 201 Created
///   + the attachment row persisted + the file physically landed
///   under <c>{BlogFilesRoot}/tester/blogs/{id}/{fileName}</c>.
///   This proves the shape the method produces is one the server
///   actually accepts — not just well-formed multipart, but the
///   contract <c>BlogApiController.PostBlog</c> binds.</description></item>
/// </list>
///
/// Built on <see cref="BlogsWebServerFixture"/> for the round-trip
/// leg (real SQLite FK enforcement, real <c>BlogSpotService</c>,
/// real <c>AddJwtBearer</c> validating <see cref="TestTokenIssuer"/>
/// tokens). The shape leg needs no host, but the class is
/// <c>[Collection("Yavsc Blogs")]</c> + <c>IClassFixture</c> so the
/// one shared host serves both legs without spinning a second one.
/// </summary>
[Collection("Yavsc Blogs")]
public sealed class CreateMultipartContentTests : IClassFixture<BlogsWebServerFixture>
{
    private readonly BlogsWebServerFixture _fixture;

    public CreateMultipartContentTests(BlogsWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>Reset the database and seed the
    /// <c>tester</c> <see cref="ApplicationUser"/> row — required
    /// for the round-trip POST, whose <c>BlogPost.AuthorId</c> is
    /// a FK to <c>AspNetUsers.Id</c> that SQLite enforces. See
    /// <see cref="BlogApiTests.ResetAndSeedDefaultUser"/> for the
    /// same rationale.</summary>
    private void ResetAndSeedDefaultUser()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("tester");
    }

    private HttpClient NewClient(string subject = "tester")
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
            new AuthenticationHeaderValue("Bearer", TestTokenIssuer.Issue(subject));
        return http;
    }

    private int CountAttachmentsForPost(long postId)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return db.BlogAttachedFiles.Count(a => a.PostId == postId);
    }

    [Fact]
    public async Task Build_produces_blog_part_camelCase_with_WhenWritingNull_and_one_file_part_per_attachment()
    {
        // A DTO that exercises both branches of the serialiser:
        // filled scalar fields (title, article, id, dates, authorId)
        // must come through in camelCase; unset graph members
        // (Author, Tags, Photo) must be dropped by
        // WhenWritingNull — the controller's
        // ReadBlogRequestAsync would otherwise receive explicit
        // nulls that confuse the resource-based ownership check.
        var post = new BlogPostDto
        {
            Id = 42,
            AuthorId = "tester",
            Author = null,
            Title = "Billet multipart",
            Article = "Contenu du billet.",
            Photo = null,
            Tags = null,
            DateCreated = new DateTime(2026, 9, 18, 12, 0, 0, DateTimeKind.Utc),
            DateModified = new DateTime(2026, 9, 18, 12, 30, 0, DateTimeKind.Utc)
        };

        var noteBytes = System.Text.Encoding.UTF8.GetBytes("payload test");
        var binBytes = new byte[] { 0x00, 0xFF, 0x42 };
        var files = new BlogUploadFile[]
        {
            new("note.txt", noteBytes, "text/plain"),
            new("bin.dat", binBytes, null) // no ContentType → octet-stream fallback
        };

        var content = BlogApiClient.CreateMultipartContent(post, files);

        // --- Top-level shape: a multipart/form-data body. ---
        var form = Assert.IsAssignableFrom<MultipartFormDataContent>(content);
        Assert.Equal("multipart/form-data", form.Headers.ContentType?.MediaType);

        // Index parts by their form field name. The "blog" field
        // is unique; the "file" field repeats once per attachment
        // (distinguished by ContentDisposition.FileName), so a
        // flat dictionary would collide — group instead.
        var byName = form
            .GroupBy(p => p.Headers.ContentDisposition?.Name?.Trim('"'))
            .ToDictionary(g => g.Key, g => g.ToList());
        Assert.Contains("blog", byName.Keys);
        Assert.Single(byName["blog"]);

        // --- "blog" part: camelCase JSON + WhenWritingNull. ---
        var blogJson = await byName["blog"][0].ReadAsStringAsync(
            TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(blogJson);
        var root = doc.RootElement;

        // camelCase: the serialiser used JsonNamingPolicy.CamelCase,
        // so the properties land as title/article/id/dateCreated/….
        Assert.True(root.TryGetProperty("title", out var titleEl));
        Assert.Equal(post.Title, titleEl.GetString());
        Assert.True(root.TryGetProperty("article", out var articleEl));
        Assert.Equal(post.Article, articleEl.GetString());
        Assert.True(root.TryGetProperty("id", out var idEl));
        Assert.Equal(post.Id, idEl.GetInt64());
        Assert.True(root.TryGetProperty("authorId", out var authorIdEl));
        Assert.Equal(post.AuthorId, authorIdEl.GetString());
        Assert.True(root.TryGetProperty("dateCreated", out _));
        Assert.True(root.TryGetProperty("dateModified", out _));

        // WhenWritingNull: the null graph members are omitted, not
        // emitted as explicit nulls.
        Assert.False(root.TryGetProperty("author", out _),
            $"Expected 'author' to be omitted (WhenWritingNull), got: {blogJson}");
        Assert.False(root.TryGetProperty("tags", out _),
            $"Expected 'tags' to be omitted (WhenWritingNull), got: {blogJson}");
        Assert.False(root.TryGetProperty("photo", out _),
            $"Expected 'photo' to be omitted (WhenWritingNull), got: {blogJson}");

        // --- "file" parts: one per attachment, right name, type,
        // and bytes. The "file" form field carries several parts;
        // collect them by filename. ---
        var fileParts = form
            .Where(p => p.Headers.ContentDisposition?.Name?.Trim('"') == "file")
            .ToDictionary(p => p.Headers.ContentDisposition?.FileName?.Trim('"'));
        Assert.Equal(2, fileParts.Count);

        var notePart = fileParts["note.txt"];
        Assert.Equal("text/plain", notePart.Headers.ContentType?.MediaType);
        Assert.Equal(noteBytes, await notePart.ReadAsByteArrayAsync(
            TestContext.Current.CancellationToken));

        // The attachment with no ContentType falls back to
        // application/octet-stream — the branch in
        // CreateMultipartContent that guards
        // string.IsNullOrWhiteSpace(file.ContentType).
        var binPart = fileParts["bin.dat"];
        Assert.Equal("application/octet-stream", binPart.Headers.ContentType?.MediaType);
        Assert.Equal(binBytes, await binPart.ReadAsByteArrayAsync(
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Build_then_POST_round_trips_through_live_host_and_persists_attachment()
    {
        ResetAndSeedDefaultUser();
        using var http = NewClient(subject: "tester");
        var filesRoot = _fixture.BlogFilesRoot;

        // Create-shape DTO (Id=0 → the controller treats it as a
        // create), mirroring what PostIt's MainPageViewModel.Save
        // builds for a first Save with an attachment.
        var draft = new BlogPostDto
        {
            Id = 0,
            AuthorId = "tester",
            Title = "Billet avec pièce jointe",
            Article = "Contenu du billet multipart.",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };
        var attachments = new BlogUploadFile[]
        {
            new("note.txt", System.Text.Encoding.UTF8.GetBytes("payload test"), "text/plain")
        };

        // The exact body PostIt would send — built by the method
        // under test, not hand-rolled here.
        using var content = BlogApiClient.CreateMultipartContent(draft, attachments);

        using var request = new HttpRequestMessage(HttpMethod.Post, _fixture.BlogSpotUrl())
        {
            Content = content
        };
        var response = await http.SendAsync(request, TestContext.Current.CancellationToken);

        // Dump the body on failure so the assertion message is
        // enough to start a fix (the framework default is opaque).
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);
            Assert.Fail(
                $"Expected 201 Created, got {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }

        var created = await response.Content.ReadFromJsonAsync<BlogPost>(
            TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.NotEqual(0, created!.Id);
        Assert.Equal("tester", created.AuthorId);

        // The attachment row is persisted…
        Assert.True(CountAttachmentsForPost(created.Id) >= 1);

        // …and the file physically landed where AttachFiles writes
        // it: {BlogFilesRoot}/{user}/blogs/{postId}/{fileName}.
        var expectedFile = Path.Combine(
            filesRoot, "tester", "blogs", created.Id.ToString(), "note.txt");
        var written = Directory.Exists(filesRoot)
            ? string.Join(", ", Directory.EnumerateFiles(filesRoot, "*", SearchOption.AllDirectories))
            : "<none>";
        Assert.True(File.Exists(expectedFile),
            $"Expected uploaded file at {expectedFile}. Actually written: {written}");
    }
}