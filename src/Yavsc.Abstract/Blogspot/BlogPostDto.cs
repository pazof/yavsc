using Yavsc.Abstract.Identity.Security;
using System.Text.Json.Serialization;

namespace Yavsc.Blogspot;

public class BlogPostDto : IBlogPost
{
    public string AuthorId { get; set; }

    public BlogPostAuthorDto? Author { get; set; }

    public string Article { get; set ; }
    public string Photo { get; set ; }
    public long Id { get; set; }
    public DateTime DateCreated { get; set; }
    public string UserCreated { get; set; }
    public DateTime DateModified { get; set; }
    public string UserModified { get; set; }
    public string Title { get; set; }

    /// <summary>
    /// Whether this post is published. Derived server-side from
    /// the existence of a row in <c>BlogSpotPublication</c>
    /// (a row means published, no row means draft). Not stored
    /// on <c>BlogPost</c> — it's a computed projection of the
    /// publication table, surfaced through the wire DTO so
    /// clients can render the current state without a
    /// follow-up request. Toggled via
    /// <c>PUT /api/BlogApi/{id}/publish</c>.
    /// </summary>
    public bool IsPublished { get; set; }

    public virtual bool AuthorizeCircle(long circleId)
    {
        ACL.Add(new CircleAuthorization { CircleId = circleId });
        return true;
    }

    public ICollection<CircleAuthorization> ACL = new List<CircleAuthorization>();

    /// <summary>
    /// Wire-only ACL bridge for System.Text.Json: accepts the
    /// <c>acl</c>/<c>ACL</c> payload from GET detail responses,
    /// but is never emitted on POST/PUT from the client.
    /// </summary>
    [JsonPropertyName("acl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<CircleAuthorization>? WireAcl
    {
        get => null;
        set => ACL = value ?? new List<CircleAuthorization>();
    }

    public string[] Tags { get; set; }

    ICollection<CircleAuthorization> ICircleAuthorized.ACL => this.ACL;

    public string[] GetTags() => Tags;

    public CircleAuthorization[] GetACL() => ACL.ToArray();
}
