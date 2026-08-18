using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Services;

/// <summary>
/// Abstraction over the central Yavsc user directory. Used by
/// the "add to a circle" flow to find Yavsc users by display
/// name or email.
///
/// <para>Distinct from <see cref="IContactService"/>, which
/// reads the device-local address book. A Yavsc user
/// directory entry is always a registered account; a device
/// contact may be anyone in the user's phone — including
/// people who have never heard of Yavsc.</para>
///
/// <para>Implementations live next to this file in
/// platform-conditional source files:
/// <c>UserDirectory.Desktop.cs</c> and
/// <c>UserDirectory.Mobile.cs</c>. Both currently delegate to
/// <c>UserSearchClient</c> (the central <c>/api/user-search</c>
/// endpoint); the split exists so future platform-specific
/// sources (offline cache, directory-scoped providers) can be
/// plugged in without disturbing the consumer.</para>
/// </summary>
public interface IUserDirectory
{
    /// <summary>
    /// Search the directory by display name (substring) and/or
    /// email (exact).
    /// </summary>
    /// <param name="query">Substring filter on the user's
    /// display name. Empty or whitespace short-circuits to an
    /// empty list (matches the client UX of "type to search",
    /// not "show me a directory").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A flat list of matching directory entries.
    /// Never null; may be empty.</returns>
    Task<IReadOnlyList<UserSummary>> SearchAsync(string query, CancellationToken ct = default);
}

/// <summary>
/// Platform-neutral summary of a Yavsc directory entry. Mirrors
/// the wire shape of <c>/api/user-search</c> (see
/// <c>UserSearchResultDto</c>) but expressed in terms that
/// don't leak transport concerns.
///
/// <para>Kept as a record on purpose: directory entries are
/// immutable snapshots from the server, so structural equality
/// makes "did the user already pick this one?" trivial.</para>
/// </summary>
public sealed record UserSummary(
    string Id,
    string UserName,
    string? FullName,
    string? Avatar,
    string? Email)
{
    /// <summary>
    /// Convenience for "what to show in a picker". Falls back
    /// to <see cref="UserName"/> when <see cref="FullName"/>
    /// is null or empty.
    /// </summary>
    public string DisplayName =>
        string.IsNullOrWhiteSpace(FullName) ? UserName : FullName;
}
