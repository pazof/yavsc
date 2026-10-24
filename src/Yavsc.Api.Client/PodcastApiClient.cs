using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
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

    public Task RetryReplayAsync(Guid id, CancellationToken ct = default)
        => api.CallAsync(HttpMethod.Post, Absolute($"{Prefix}/{id}/replay/retry"), ct: ct);

    public async Task<byte[]> DownloadReplayRangeAsync(Guid id, long offset, int count, long size,
        CancellationToken ct = default)
    {
        if (offset < 0 || count <= 0 || count > 256 * 1024 || offset >= size)
            throw new ArgumentOutOfRangeException(nameof(offset), "Invalid replay byte range.");
        var end = offset + Math.Min(count, size - offset) - 1;
        using var response = await api.SendAuthenticatedAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Absolute($"{Prefix}/{id}/replay.mp4"));
            request.Headers.Range = new RangeHeaderValue(offset, end);
            return request;
        }, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var range = response.Content.Headers.ContentRange;
        if (response.StatusCode != HttpStatusCode.PartialContent || range?.Unit != "bytes"
            || range.From != offset || range.To != end || range.Length != size)
            throw new InvalidDataException("Le serveur n'a pas respecte la plage demandee pour le replay.");
        var length = checked((int)(end - offset + 1));
        var bytes = new byte[length];
        await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        await stream.ReadExactlyAsync(bytes, ct).ConfigureAwait(false);
        var extra = new byte[1];
        if (await stream.ReadAsync(extra.AsMemory(), ct).ConfigureAwait(false) != 0)
            throw new InvalidDataException("Reponse MP4 plus grande que la plage demandee.");
        return bytes;
    }

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
