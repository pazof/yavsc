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
}
