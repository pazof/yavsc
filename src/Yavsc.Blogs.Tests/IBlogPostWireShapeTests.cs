using System.Text.Json;
using Yavsc.Blogspot;
using Yavsc.Models.Blog;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Wire-shape tests pinning that <see cref="IBlogPost.IsPublished"/>
/// is carried on the JSON the blog API list endpoint emits.
///
/// <para>The blog API host (<c>Yavsc.Blogs/Program.cs</c>) serialises
/// with System.Text.Json (the <c>.AddControllers()</c> default — no
/// <c>AddNewtonsoftJson</c>), and <c>BlogApiController.GetBlogspot</c>
/// returns <c>IEnumerable&lt;IBlogPost&gt;</c>. System.Text.Json emits
/// only the members of the <b>declared</b> type, so a property that
/// exists solely on the concrete <c>BlogPost</c> entity is dropped from
/// the payload. <c>IsPublished</c> used to be one such property: it was
/// on <c>BlogPost</c> and <c>BlogPostDto</c> but not on
/// <see cref="IBlogPost"/>, so the list response omitted it and PostIt
/// read <c>IsPublished == false</c> for every post — including
/// published ones.</para>
///
/// <para>These tests reproduce that serialisation path directly
/// (declared type <c>IEnumerable&lt;IBlogPost&gt;</c> through
/// <see cref="JsonSerializer.Serialize(object, Type)"/>) and assert
/// <c>isPublished</c> survives the round trip. They live in
/// <c>Yavsc.Blogs.Tests</c> — the assembly that owns the blog API
/// contract — so a regression at the wire boundary is caught here,
/// where it manifested in production (PostIt showing a published
/// post's status as <c>false</c>).</para>
/// </summary>
public sealed class IBlogPostWireShapeTests
{
    private static readonly JsonSerializerOptions WebDefaults
        = new(JsonSerializerDefaults.Web);

    [Fact]
    public void IsPublished_is_emitted_when_list_is_serialised_as_IEnumerable_IBlogPost()
    {
        var post = new BlogPost
        {
            Id = 13,
            Title = "Published",
            AuthorId = "alice",
            Article = "x",
        };
        post.IsPublished = true;

        // Declared type mirrors GetBlogspot's return: IEnumerable<IBlogPost>.
        IEnumerable<IBlogPost> list = new List<BlogPost> { post };

        // Serialize with the DECLARED type, the way ASP.NET Core's
        // SystemTextJsonOutputFormatter hands context.ObjectType (the
        // action return type) to JsonSerializer.Serialize.
        var json = JsonSerializer.Serialize(list, typeof(IEnumerable<IBlogPost>),
            WebDefaults);

        using var doc = JsonDocument.Parse(json);
        var element = doc.RootElement[0];
        // Web defaults use camelCase, so the property is "isPublished".
        Assert.True(element.TryGetProperty("isPublished", out var isPublished),
            $"isPublished was dropped from the list payload. JSON: {json}");
        Assert.True(isPublished.GetBoolean());
    }

    [Fact]
    public void IsPublished_round_trips_through_BlogPostDto_on_the_client()
    {
        // The shape the client (PostIt) receives and deserialises with
        // PropertyNameCaseInsensitive = true (cf. YavscApiClient).
        var json = """
        [{
          "id": 13,
          "title": "Published",
          "article": "x",
          "authorId": "alice",
          "isPublished": true
        }]
        """;

        var posts = JsonSerializer.Deserialize<List<BlogPostDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var post = Assert.Single(posts);
        Assert.True(post.IsPublished);
    }
}