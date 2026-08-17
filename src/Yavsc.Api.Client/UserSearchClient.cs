using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Api.Client.Dtos;

namespace Yavsc.Api.Client;

/// <summary>
/// HTTP client for <c>/api/user-search</c> on the Yavsc Blogs
/// server. Used by client-side address books (PostIt.Desktop,
/// future PostIt.Browser CLI, …) to look up Yavsc users by
/// display name or email.
///
/// <para>The server scopes every endpoint to the authenticated
/// caller; any authenticated user can search the user table of
/// the instance. There is no per-user filtering on the response
/// side — this is by design on single-tenant deployments
/// (closed community). Multi-tenant deployments should gate
/// this controller behind a tenant-scoped policy before
/// exposing it; see the server-side
/// <c>UserSearchApiController</c> doc for details.</para>
/// </summary>
public sealed class UserSearchClient
{
    private const string Path = "user-search";

    private readonly IYavscApiClient _api;

    public UserSearchClient(IYavscApiClient api, string blogsBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        if (string.IsNullOrEmpty(blogsBaseAddress))
            throw new ArgumentException("Base address is required.", nameof(blogsBaseAddress));

        if (api.Http.BaseAddress is null)
            api.Http.BaseAddress = new Uri(blogsBaseAddress);
    }

    /// <summary>
    /// Search users by display name (substring) or email (exact).
    /// </summary>
    /// <param name="query">Substring filter on FullName or
    /// UserName. Empty or null returns an empty list (the server
    /// would return all users, which we don't want by
    /// default).</param>
    /// <param name="email">Optional exact-match filter on
    /// Email.</param>
    /// <param name="take">Maximum results, capped at 100.
    /// Default 25.</param>
    public Task<List<UserSearchResultDto>> SearchAsync(
        string? query = null,
        string? email = null,
        int take = 25,
        CancellationToken ct = default)
    {
        // Match the server's contract: at least one filter is
        // expected. The server doesn't enforce this (an empty
        // query + empty email returns the first `take` users
        // alphabetically), but the address-book UX is "type
        // something to search", so we short-circuit empty
        // queries client-side.
        if (string.IsNullOrWhiteSpace(query) && string.IsNullOrWhiteSpace(email))
            return Task.FromResult(new List<UserSearchResultDto>());

        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(query))
            qs.Add($"q={Uri.EscapeDataString(query)}");
        if (!string.IsNullOrWhiteSpace(email))
            qs.Add($"e={Uri.EscapeDataString(email)}");
        qs.Add($"take={Math.Clamp(take, 1, 100)}");

        return _api.CallAsync<List<UserSearchResultDto>>(
            HttpMethod.Get,
            $"{Path}?{string.Join('&', qs)}",
            ct: ct);
    }
}