using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Abstract.Identity.Security;
using Yavsc.Api.Client.Dtos;

namespace Yavsc.Api.Client;

/// <summary>
/// HTTP client for <c>/api/blogacl</c> on the Yavsc Blogs server.
///
/// <para>Each <see cref="CircleAuthorization"/> grants a single
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

    public Task<List<CircleAuthorization>> GetMyAclAsync(CancellationToken ct = default)
        => _api.CallAsync<List<CircleAuthorization>>(HttpMethod.Get, Path, ct: ct);

    public Task<CircleAuthorization?> GetAclAsync(long circleId, CancellationToken ct = default)
        => _api.CallAsync<CircleAuthorization?>(HttpMethod.Get, $"{Path}/{circleId}", ct: ct);

    public Task<CircleAuthorization?> GrantAsync(CircleAuthorization acl, CancellationToken ct = default)
        => _api.CallAsync<CircleAuthorization?>(HttpMethod.Post, Path, body: acl, ct: ct);

    public Task UpdateAclAsync(long circleId, CircleAuthorization acl, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Put, $"{Path}/{circleId}", body: acl, ct: ct);

    public Task RevokeAsync(long circleId, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Delete, $"{Path}/{circleId}", ct: ct);
}
