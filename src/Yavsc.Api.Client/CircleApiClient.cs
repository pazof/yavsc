using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Api.Client.Dtos;

namespace Yavsc.Api.Client;

/// <summary>
/// HTTP client for <c>/api/circle</c> on the Yavsc Blogs server.
///
/// <para>Same conventions as <see cref="BlogApiClient"/>: all
/// transport is delegated to <see cref="YavscApiClient"/>; this
/// class only maps paths to DTOs.</para>
///
/// <para>The server now (since the BlogAcl fix on this branch)
/// scopes every read and write to the caller's uid. There is no
/// way for the client to read or modify another user's circles
/// — the route will return 404 (not 403) when the circle exists
/// but belongs to someone else, to avoid leaking its existence.</para>
/// </summary>
public sealed class CircleApiClient
{
    private const string Path = "circle";

    private readonly IYavscApiClient _api;

    public CircleApiClient(IYavscApiClient api, string blogsBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        if (string.IsNullOrEmpty(blogsBaseAddress))
            throw new ArgumentException("Base address is required.", nameof(blogsBaseAddress));

        if (api.Http.BaseAddress is null)
            api.Http.BaseAddress = new Uri(blogsBaseAddress);
    }

    public Task<List<CircleDto>> GetMyCirclesAsync(CancellationToken ct = default)
        => _api.CallAsync<List<CircleDto>>(HttpMethod.Get, Path, ct: ct);

    public Task<CircleDto?> GetCircleAsync(long id, CancellationToken ct = default)
        => _api.CallAsync<CircleDto?>(HttpMethod.Get, $"{Path}/{id}", ct: ct);

    public Task<CircleDto?> CreateCircleAsync(CircleDto circle, CancellationToken ct = default)
        => _api.CallAsync<CircleDto?>(HttpMethod.Post, Path, body: circle, ct: ct);

    public Task UpdateCircleAsync(long id, CircleDto circle, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Put, $"{Path}/{id}", body: circle, ct: ct);

    public Task DeleteCircleAsync(long id, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Delete, $"{Path}/{id}", ct: ct);

    /// <summary>
    /// Returns the members of one of the caller's circles.
    /// Returns null when the circle does not exist or is not
    /// owned by the caller (the server scopes the endpoint
    /// with a 404 in either case to avoid leaking existence
    /// — this client flattens that into a null result).
    /// </summary>
    public Task<List<CircleMemberDto>?> GetMembersAsync(long id, CancellationToken ct = default)
        => _api.CallAsync<List<CircleMemberDto>?>(HttpMethod.Get, $"{Path}/{id}/members", ct: ct);

    /// <summary>
    /// Adds a Yavsc user (resolved client-side via
    /// <c>/api/user-search</c>) to one of the caller's
    /// circles. Returns null when the circle does not exist
    /// or is not owned by the caller, or when the target
    /// user does not exist. Throws on 409 (already a
    /// member) — callers that want idempotent behaviour
    /// can swallow the exception or dedupe beforehand.
    /// </summary>
    public Task AddMemberAsync(long id, string userId, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Post, $"{Path}/{id}/members",
            body: new { userId }, ct: ct);

    /// <summary>
    /// Removes a user from one of the caller's circles.
    /// Returns null on success (the server returns 200 OK
    /// with no body) or when the membership does not
    /// exist — both treated as success by the caller.
    /// </summary>
    public Task RemoveMemberAsync(long id, string userId, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Delete, $"{Path}/{id}/members/{userId}", ct: ct);
}
