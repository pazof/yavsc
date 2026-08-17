namespace Yavsc.Api.Client.Dtos;

/// <summary>
/// Wire format for <c>GET /api/user-search</c>.
///
/// <para>Mirrors the server-side
/// <c>Yavsc.Blogs.Controllers.UserSearchResultDto</c> but stops
/// short of any entity navigation properties. Only the fields
/// a client address book needs (id, name, avatar, email) are
/// included.</para>
///
/// <para>Field names match the JSON the server emits (camelCase
/// via the default <see cref="System.Text.Json"/> policy), so
/// no <c>[JsonPropertyName]</c> attributes are required.</para>
/// </summary>
public sealed class UserSearchResultDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
}