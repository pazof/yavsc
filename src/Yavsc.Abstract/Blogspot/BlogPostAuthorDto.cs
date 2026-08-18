namespace Yavsc.Blogspot;

/// <summary>
/// Minimum-viable author payload embedded in <see cref="BlogPostDto"/>.
///
/// <para>
/// Before this record existed, <c>BlogPostDto.Author</c> was typed
/// as the abstract interface <c>IApplicationUser</c>. The
/// interface is fine for server-side contract (we have a concrete
/// entity that implements it) but System.Text.Json cannot
/// materialise an interface without a polymorphic converter
/// configured on both ends. PostIt would crash on load-posts
/// because the JSON contained an <c>Author</c> object that the
/// client could not deserialise.
/// </para>
///
/// <para>
/// This record is the wire shape: <c>Id</c> for "go to author
/// profile", <c>UserName</c> for "by @username", <c>Avatar</c>
/// for the round badge next to the title. The server-side
/// <c>BlogPost</c> entity (<c>Yavsc.Server.Models.Blog</c>) keeps
/// its full <c>ApplicationUser</c> navigation property for
/// permission checks and authorisation; the DTO is built on
/// demand by the controller / service layer when the post is
/// served to the wire.
/// </para>
/// </summary>
public sealed record BlogPostAuthorDto
{
    public string Id { get; init; } = string.Empty;
    public string? UserName { get; init; }
    public string? Avatar { get; init; }
}
