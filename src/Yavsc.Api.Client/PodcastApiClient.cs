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
        => CreateAsync(new CreatePodcastRequest { Title = title }, ct);

    /// <summary>Starts an immediate live, optionally refined with its retention.</summary>
    public Task<PodcastResponse> CreateAsync(CreatePodcastRequest request, CancellationToken ct = default)
        => api.CallAsync<PodcastResponse>(HttpMethod.Post, Absolute(Prefix), request, ct);

    public Task<PodcastResponse> PrepareAsync(PreparePodcastRequest request, Guid? id = null, CancellationToken ct = default)
        => api.CallAsync<PodcastResponse>(id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            Absolute(id.HasValue ? $"{Prefix}/{id}/planned" : $"{Prefix}/planned"), request, ct);

    public Task<PodcastResponse> StartAsync(Guid id, CancellationToken ct = default)
        => api.CallAsync<PodcastResponse>(HttpMethod.Post, Absolute($"{Prefix}/{id}/start"), ct: ct);

    public Task<PodcastObsCredentials> ActivateObsAsync(Guid id, CancellationToken ct = default)
        => api.CallAsync<PodcastObsCredentials>(HttpMethod.Post, Absolute($"{Prefix}/{id}/obs"), ct: ct);

    public Task<PodcastObsStatus> ObsStatusAsync(Guid id, CancellationToken ct = default)
        => api.CallAsync<PodcastObsStatus>(HttpMethod.Get, Absolute($"{Prefix}/{id}/obs"), ct: ct);

    public Task DeactivateObsAsync(Guid id, CancellationToken ct = default)
        => api.CallAsync(HttpMethod.Delete, Absolute($"{Prefix}/{id}/obs"), ct: ct);

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

    /// <summary>Saves the marks and removed ranges of the owned draft; a stale revision fails with 409.</summary>
    public Task<PodcastResponse> SaveDraftAsync(Guid id, SavePodcastDraftRequest request, CancellationToken ct = default)
        => api.CallAsync<PodcastResponse>(HttpMethod.Put, Absolute($"{Prefix}/{id}/draft"), request, ct);

    /// <summary>Queues the publication of a saved draft revision; repeating it is harmless.</summary>
    public Task<PodcastResponse> PublishDraftAsync(Guid id, int revision, CancellationToken ct = default)
        => api.CallAsync<PodcastResponse>(HttpMethod.Post, Absolute($"{Prefix}/{id}/draft/publish"),
            new PublishPodcastDraftRequest { Revision = revision }, ct);

    /// <summary>Deletes the originals and draft of a published refined replay; the MP4 remains.</summary>
    public Task<PodcastResponse> DeleteOriginalsAsync(Guid id, CancellationToken ct = default)
        => api.CallAsync<PodcastResponse>(HttpMethod.Delete, Absolute($"{Prefix}/{id}/originals"), ct: ct);

    public Task<byte[]> DownloadReplayRangeAsync(Guid id, long offset, int count, long size,
        CancellationToken ct = default)
        => DownloadRangeAsync($"{Prefix}/{id}/replay.mp4", offset, count, size, ct);

    /// <summary>Reads a byte range of the owner-only seekable source of a refined replay draft.</summary>
    public Task<byte[]> DownloadDraftSourceRangeAsync(Guid id, long offset, int count, long size,
        CancellationToken ct = default)
        => DownloadRangeAsync($"{Prefix}/{id}/draft/source.mp4", offset, count, size, ct);

    private async Task<byte[]> DownloadRangeAsync(string path, long offset, int count, long size, CancellationToken ct)
    {
        if (offset < 0 || count <= 0 || count > 256 * 1024 || offset >= size)
            throw new ArgumentOutOfRangeException(nameof(offset), "Invalid replay byte range.");
        var end = offset + Math.Min(count, size - offset) - 1;
        using var response = await api.SendAuthenticatedAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Absolute(path));
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
