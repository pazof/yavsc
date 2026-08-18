namespace Yavsc.Api.Client.Dtos;

/// <summary>
/// Wire format for <c>GET /api/circle/{id}/members</c>.
///
/// <para>Mirrors the server-side
/// <c>Yavsc.Blogs.Controllers.CircleMemberDto</c>. Intentionally
/// stops short of the Email field that
/// <see cref="UserSearchResultDto"/> carries — the circle
/// membership UI only needs a name and an avatar to render the
/// list. If the future ACL UI wants contact details, it can
/// fall back to <see cref="IYavscApiClient"/>'s other
/// endpoints rather than widening this shape.</para>
/// </summary>
public sealed class CircleMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
}
