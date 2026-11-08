#nullable enable
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Yavsc.Api.Services;

public sealed record PodcastObsPublisher(Guid PodcastId, Guid ConnectionId, bool Secure);

public interface IPodcastObsRelay
{
    Task<IReadOnlyList<PodcastObsPublisher>> PublishersAsync(CancellationToken ct);
    Task KickAsync(Guid id, CancellationToken ct, Guid? connectionId = null);
}

public sealed class PodcastObsRelay(HttpClient http, IOptions<PodcastObsOptions> options) : IPodcastObsRelay
{
    private HttpRequestMessage Request(HttpMethod method, string path)
    {
        var settings = options.Value;
        settings.Validate();
        var request = new HttpRequestMessage(method, new Uri(new Uri(settings.RelayApiUrl.TrimEnd('/') + "/"), path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.RelayUser}:{settings.RelayPassword}")));
        return request;
    }

    public async Task<IReadOnlyList<PodcastObsPublisher>> PublishersAsync(CancellationToken ct)
    {
        var publishers = new List<PodcastObsPublisher>();
        var page = 0;
        while (true)
        {
            using var request = Request(HttpMethod.Get, $"v3/paths/list?itemsPerPage=100&page={page}");
            using var response = await http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            foreach (var item in document.RootElement.GetProperty("items").EnumerateArray())
                if (item.GetProperty("ready").GetBoolean()
                    && PodcastObsService.TryId(item.GetProperty("name").GetString() ?? "", out var id)
                    && item.TryGetProperty("source", out var source) && source.ValueKind == JsonValueKind.Object
                    && source.GetProperty("type").GetString() is "rtmpConn" or "rtmpsConn"
                    && Guid.TryParse(source.GetProperty("id").GetString(), out var connection))
                    publishers.Add(new(id, connection, source.GetProperty("type").GetString() == "rtmpsConn"));
            if (++page >= document.RootElement.GetProperty("pageCount").GetInt32()) break;
        }
        return publishers;
    }

    public async Task KickAsync(Guid id, CancellationToken ct, Guid? connectionId = null)
    {
        foreach (var publisher in (await PublishersAsync(ct)).Where(p => p.PodcastId == id
            && (!connectionId.HasValue || p.ConnectionId == connectionId)))
        {
            using var request = Request(HttpMethod.Post, $"v3/{(publisher.Secure ? "rtmps" : "rtmp")}/conns/kick/{publisher.ConnectionId}");
            using var response = await http.SendAsync(request, ct);
            if (response.StatusCode != HttpStatusCode.NotFound) response.EnsureSuccessStatusCode();
        }
    }
}
