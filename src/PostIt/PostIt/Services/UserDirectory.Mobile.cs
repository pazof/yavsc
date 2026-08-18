#if ANDROID || IOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Api.Client;

namespace PostIt.Services;

/// <summary>
/// Mobile implementation of <see cref="IUserDirectory"/>.
/// Same backing as the desktop provider (the central
/// <c>/api/user-search</c> endpoint via
/// <see cref="UserSearchClient"/>) — mobile devices have the
/// network too, and "add to a circle" needs the same directory
/// regardless of platform.
///
/// <para>The split exists so a future mobile-only provider
/// (offline cache, device-local mirror of the user's own
/// circles) can be plugged in without touching consumers.</para>
/// </summary>
public sealed class UserDirectory : IUserDirectory
{
    private readonly UserSearchClient _client;

    public UserDirectory(UserSearchClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<IReadOnlyList<UserSummary>> SearchAsync(
        string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<UserSummary>();

        var results = await _client.SearchAsync(query: query, ct: ct).ConfigureAwait(false);
        if (results is null) return Array.Empty<UserSummary>();

        return results.Select(u => new UserSummary(
            Id: u.Id,
            UserName: u.UserName,
            FullName: u.FullName,
            Avatar: u.Avatar,
            Email: u.Email)).ToList();
    }
}
#endif
