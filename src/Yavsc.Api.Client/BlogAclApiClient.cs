using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Abstract.BlogSpot;
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

    public Task<List<PostAccessControlRulePayload>> GetMyAclAsync(CancellationToken ct = default)
        => _api.CallAsync<List<PostAccessControlRulePayload>>(HttpMethod.Get, Path, ct: ct);

    public Task<PostAccessControlRulePayload?> GetAclAsync(long circleId, CancellationToken ct = default)
        => _api.CallAsync<PostAccessControlRulePayload?>(HttpMethod.Get, $"{Path}/{circleId}", ct: ct);

    public Task<PostAccessControlRulePayload?> GrantAsync(PostAccessControlRulePayload acl, CancellationToken ct = default)
        => _api.CallAsync<PostAccessControlRulePayload?>(HttpMethod.Post, Path, body: acl, ct: ct);

    public Task UpdateAclAsync(long circleId, PostAccessControlRulePayload acl, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Put, $"{Path}/{circleId}", body: acl, ct: ct);

    public Task RevokeAsync(long circleId, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Delete, $"{Path}/{circleId}", ct: ct);
}
