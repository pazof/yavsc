namespace Yavsc.Api.Client.Dtos;

/// <summary>
/// Wire format for <c>GET /api/circle</c> and friends.
///
/// <para>Field names match the JSON the server emits (camelCase via
/// the default <see cref="System.Text.Json"/> policy), so no
/// <c>[JsonPropertyName]</c> attributes are required.</para>
///
/// <para>Mirrors the server-side <c>Yavsc.Models.Relationship.Circle</c>
/// EF entity but stops short of the navigation properties
/// (<c>Owner</c>, <c>Members</c>) which depend on
/// <c>ApplicationUser</c> and other server-only types. The client
/// only ever needs the id, name, and owner of a circle to drive
/// the UI.</para>
/// </summary>
public sealed class CircleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public bool Public { get; set; }
}
