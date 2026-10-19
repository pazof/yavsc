using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Abstract.Streaming;

namespace Yavsc.Api.Client;

public sealed class PodcastApiClient(IYavscApiClient api, Func<string> baseAddress)
{
    private const string Prefix = "podcasts";

    public Task<List<PodcastResponse>> ListAsync(CancellationToken ct = default)
        => api.CallAsync<List<PodcastResponse>>(HttpMethod.Get, Absolute(Prefix), ct: ct);

    public Task<PodcastResponse> CreateAsync(string title, CancellationToken ct = default)
        => api.CallAsync<PodcastResponse>(HttpMethod.Post, Absolute(Prefix), new CreatePodcastRequest { Title = title }, ct);

    public Task<PodcastManifestResponse> GetAsync(Guid id, CancellationToken ct = default)
        => api.CallAsync<PodcastManifestResponse>(HttpMethod.Get, Absolute($"{Prefix}/{id}"), ct: ct);

    public Task<PodcastResponse> UploadAsync(Guid id, int sequence, double duration, byte[] bytes, CancellationToken ct = default)
        => api.CallAsync<PodcastResponse>(HttpMethod.Put,
            Absolute($"{Prefix}/{id}/segments/{sequence}?duration={duration.ToString("R", CultureInfo.InvariantCulture)}"),
            () =>
            {
                var content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("video/mp2t");
                return content;
            }, ct);

    public Task<PodcastResponse> CompleteAsync(Guid id, CancellationToken ct = default)
        => api.CallAsync<PodcastResponse>(HttpMethod.Post, Absolute($"{Prefix}/{id}/complete"), ct: ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => api.CallAsync(HttpMethod.Delete, Absolute($"{Prefix}/{id}"), ct: ct);

    public Task<byte[]> DownloadSegmentAsync(Guid id, int sequence, CancellationToken ct = default)
        => api.DownloadAsync(HttpMethod.Get, Absolute($"{Prefix}/{id}/segments/{sequence}.ts"), ct);

    private string Absolute(string path)
    {
        var address = new Uri(baseAddress().TrimEnd('/') + "/", UriKind.Absolute);
        if (!address.AbsolutePath.EndsWith("/" + Constants.APIPrefix + "/", StringComparison.Ordinal))
            address = new Uri(address, Constants.APIPrefix + "/");
        return new Uri(address, path).AbsoluteUri;
    }
}
