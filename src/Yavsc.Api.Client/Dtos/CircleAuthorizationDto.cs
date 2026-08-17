namespace Yavsc.Api.Client.Dtos;

/// <summary>
/// Wire format for <c>GET /api/blogacl</c> and friends.
///
/// <para>The server-side
/// <c>Yavsc.Models.Access.CircleAuthorizationToBlogPost</c> EF entity
/// carries virtual navigation properties (<c>Target</c>,
/// <c>Allowed</c>) that pull in the full BlogPost and Circle graphs.
/// The client never needs them: when showing the ACL of a post, the
/// UI already has the post, and the circles are looked up by id
/// against the list returned by <c>GET /api/circle</c>.</para>
/// </summary>
public sealed class CircleAuthorizationDto
{
    public long CircleId { get; set; }
    public long BlogPostId { get; set; }
    public bool Comment { get; set; }
}
