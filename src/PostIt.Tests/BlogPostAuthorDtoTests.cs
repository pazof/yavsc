using System.Text.Json;
using Yavsc.Blogspot;

namespace PostIt.Tests;

/// <summary>
/// Round-trip tests for the wire shape of a blog post as
/// serialised by Yavsc.Blogs and consumed by PostIt.
///
/// <para>
/// Background: in 1.0.7, <c>BlogPostDto.Author</c> was typed as
/// the abstract interface <c>IApplicationUser</c>. System.Text.Json
/// cannot materialise an interface without a polymorphic
/// converter, so the "load posts" call from PostIt crashed when
/// the server returned a post with a populated <c>Author</c>
/// object. The fix replaced <c>IApplicationUser</c> with a thin
/// concrete DTO, <c>BlogPostAuthorDto</c>, embedded directly in
/// <c>BlogPostDto.Author</c>.
/// </para>
///
/// <para>
/// These tests pin the wire shape: a JSON document with an
/// <c>Author</c> object must deserialise without throwing and
/// must round-trip the three fields PostIt exposes in the UI
/// (Id, UserName, Avatar). They are intentionally placed in
/// <c>PostIt.Tests</c> — the client-side assembly — so the
/// regression is caught at the deserialisation boundary, where
/// it actually manifested in production.
/// </para>
/// </summary>
public class BlogPostAuthorDtoTests
{
    private static readonly JsonSerializerOptions CaseInsensitiveJson
        = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void BlogPostDto_deserialises_with_populated_author()
    {
        // A representative JSON shape the server would emit for
        // GET /api/BlogApi. The Author object is fully populated
        // — that's the shape that used to break deserialisation
        // when Author was typed as the abstract IApplicationUser
        // interface.
        var json = """
        {
          "id": 42,
          "title": "Premier billet",
          "article": "Contenu",
          "photo": null,
          "dateCreated": "2026-08-01T12:00:00Z",
          "dateModified": "2026-08-02T12:00:00Z",
          "userCreated": "alice",
          "userModified": "alice",
          "authorId": "u-alice",
          "isPublished": true,
          "author": {
            "id": "u-alice",
            "userName": "alice",
            "avatar": "/avatars/alice.png"
          }
        }
        """;

        var post = JsonSerializer.Deserialize<BlogPostDto>(json, CaseInsensitiveJson);

        Assert.NotNull(post);
        Assert.Equal(42, post!.Id);
        Assert.Equal("Premier billet", post.Title);
        Assert.Equal("u-alice", post.AuthorId);
        Assert.True(post.IsPublished);

        // The actual regression coverage: Author must
        // materialise as a concrete DTO, not be left null because
        // of a JsonException on IApplicationUser.
        Assert.NotNull(post.Author);
        Assert.Equal("u-alice", post.Author!.Id);
        Assert.Equal("alice", post.Author.UserName);
        Assert.Equal("/avatars/alice.png", post.Author.Avatar);
    }

    [Fact]
    public void BlogPostDto_deserialises_when_author_is_null()
    {
        // The server is allowed to omit Author (the field is
        // nullable on the wire — it maps to a navigation
        // property that may not have been Included). The client
        // must accept that shape without throwing.
        var json = """
        {
          "id": 7,
          "title": "Sans auteur",
          "article": null,
          "photo": null,
          "dateCreated": "2026-08-01T12:00:00Z",
          "dateModified": "2026-08-01T12:00:00Z",
          "userCreated": "system",
          "userModified": "system",
          "authorId": "system",
          "isPublished": false,
          "author": null
        }
        """;

        var post = JsonSerializer.Deserialize<BlogPostDto>(json, CaseInsensitiveJson);

        Assert.NotNull(post);
        Assert.Null(post!.Author);
        Assert.Equal("system", post.AuthorId);
    }

    [Fact]
    public void BlogPostDto_deserialises_when_author_field_is_missing()
    {
        // Forward-compatibility: an older server that doesn't
        // emit the Author field at all. Should not throw.
        var json = """
        {
          "id": 9,
          "title": "Ancien format",
          "article": "Pas d'auteur dans la charge utile",
          "photo": null,
          "dateCreated": "2026-07-01T12:00:00Z",
          "dateModified": "2026-07-01T12:00:00Z",
          "userCreated": "bob",
          "userModified": "bob",
          "authorId": "u-bob",
          "isPublished": true
        }
        """;

        var post = JsonSerializer.Deserialize<BlogPostDto>(json, CaseInsensitiveJson);

        Assert.NotNull(post);
        Assert.Null(post!.Author);
    }

    [Fact]
    public void BlogPostAuthorDto_serialises_back_to_expected_json_shape()
    {
        // Pin the wire shape on the way out too. The server
        // builds BlogPostAuthorDto from an ApplicationUser and
        // PostIt receives it as JSON; if the field names
        // change (e.g. case) the round-trip on the client side
        // is what would silently break.
        //
        // The server emits camelCase (ASP.NET Core's Web
        // defaults — PropertyNamingPolicy = CamelCase). We
        // mirror that here so the test reflects what the wire
        // actually looks like. PropertyNameCaseInsensitive on
        // the client deserialiser means we don't have to
        // hardcode the casing for the inbound assertions.
        var author = new BlogPostAuthorDto
        {
            Id = "u-alice",
            UserName = "alice",
            Avatar = "/avatars/alice.png"
        };

        var json = JsonSerializer.Serialize(author,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("id", out _));
        Assert.True(root.TryGetProperty("userName", out _));
        Assert.True(root.TryGetProperty("avatar", out _));
    }
}
