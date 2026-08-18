#if !ANDROID && !IOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Api.Client;

namespace PostIt.Services;

/// <summary>
/// Desktop implementation of <see cref="IUserDirectory"/>.
/// Delegates to the central <c>/api/user-search</c> endpoint
/// via <see cref="UserSearchClient"/>.
///
/// <para>The desktop has no device-local address book, so the
/// "add to a circle" flow on desktop is Yavsc-users-only.
/// Inviting someone who doesn't have a Yavsc account from
/// desktop is a separate feature (manual email entry +
/// invitation endpoint) and lives outside this interface.</para>
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
        // UserSearchClient already short-circuits on empty
        // queries, but do it here too so the contract is
        // obvious to anyone reading IUserDirectory alone
        // without having to chase the client wrapper.
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
