using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Api.Client.Dtos;

namespace Yavsc.Api.Client;

/// <summary>
/// HTTP client for <c>/api/blogacl</c> on the Yavsc Blogs server.
///
/// <para>Each <see cref="CircleAuthorizationDto"/> grants a single
/// <c>Circle</c> access to a single <c>BlogPostDto</c>. The server
/// scopes every endpoint to the caller's uid: only the author of
/// the underlying blog post can list, create, modify, or delete
/// its ACL entries.</para>
/// </summary>
public sealed class BlogAclApiClient
{
    private const string Path = "blogacl";

    private readonly IYavscApiClient _api;

    public BlogAclApiClient(IYavscApiClient api, string blogsBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        if (string.IsNullOrEmpty(blogsBaseAddress))
            throw new ArgumentException("Base address is required.", nameof(blogsBaseAddress));

        if (api.Http.BaseAddress is null)
            api.Http.BaseAddress = new Uri(blogsBaseAddress);
    }

    public Task<List<CircleAuthorizationDto>> GetMyAclAsync(CancellationToken ct = default)
        => _api.CallAsync<List<CircleAuthorizationDto>>(HttpMethod.Get, Path, ct: ct);

    public Task<CircleAuthorizationDto?> GetAclAsync(long circleId, CancellationToken ct = default)
        => _api.CallAsync<CircleAuthorizationDto?>(HttpMethod.Get, $"{Path}/{circleId}", ct: ct);

    public Task<CircleAuthorizationDto?> GrantAsync(CircleAuthorizationDto acl, CancellationToken ct = default)
        => _api.CallAsync<CircleAuthorizationDto?>(HttpMethod.Post, Path, body: acl, ct: ct);

    public Task UpdateAclAsync(long circleId, CircleAuthorizationDto acl, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Put, $"{Path}/{circleId}", body: acl, ct: ct);

    public Task RevokeAsync(long circleId, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Delete, $"{Path}/{circleId}", ct: ct);
}
