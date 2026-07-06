using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PostIt.Models;

namespace PostIt.Services;

/// <summary>
/// High-level client for the Blog subsystem of the Yavsc API
/// (deployed at <c>https://blogs.pschneider.fr</c>). All transport
/// concerns — base URL, JSON serialisation, Bearer auth, silent
/// refresh on 401, request body shaping — are delegated to
/// <see cref="YavscApiClient"/>. This class is a thin DTO↔path
/// mapper, nothing more.
///
/// <para><b>URL convention.</b> <see cref="YavscApiClient"/>'s
/// <c>BaseAddress</c> already terminates with <c>/api/v1/</c>
/// (see <c>Settings.ApiUrl</c>). The path prefix below is
/// therefore <i>relative</i> to that version segment: a prefix of
/// <c>"blog"</c> resolves to <c>…/api/v1/blog</c>, which matches
/// the <c>[Route(APIPrefix + "/blog")]</c> attribute on
/// <c>Yavsc.Blogs.Controllers.BlogApiController</c>. Do not
/// re-include the <c>api/</c> segment here — that produced 404s
/// in the past (see commit "PostIt: fix blog API double-prefix").</para>
///
/// The class is intentionally non-IDisposable: it does not own the
/// <see cref="YavscApiClient"/> it depends on. Lifetimes are managed
/// by the consumer (typically a singleton service registered with
/// the application).
/// </summary>
public sealed class BlogApiClient
{
    private const string DefaultPathPrefix = "blog";

    private readonly YavscApiClient _api;
    private readonly string _pathPrefix;

    public BlogApiClient(YavscApiClient api, string pathPrefix = DefaultPathPrefix)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _pathPrefix = pathPrefix?.TrimStart('/') ?? DefaultPathPrefix;
    }

    public Task<List<BlogPost>> GetPostsAsync(int start = 0, int take = 25, CancellationToken ct = default)
        => _api.CallAsync<List<BlogPost>>(
            HttpMethod.Get,
            $"{_pathPrefix}?start={start}&take={take}",
            ct: ct);

    public Task<BlogPost?> GetPostAsync(long id, CancellationToken ct = default)
        => _api.CallAsync<BlogPost?>(HttpMethod.Get, $"{_pathPrefix}/{id}", ct: ct);

    public Task<BlogPost?> CreatePostAsync(BlogPost post, CancellationToken ct = default)
        => _api.CallAsync<BlogPost?>(HttpMethod.Post, _pathPrefix, body: post, ct: ct);

    public Task UpdatePostAsync(long id, BlogPost post, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Put, $"{_pathPrefix}/{id}", body: post, ct: ct);

    public Task DeletePostAsync(long id, CancellationToken ct = default)
        => _api.CallAsync(HttpMethod.Delete, $"{_pathPrefix}/{id}", ct: ct);
}
