using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Yavsc.Models;
using Yavsc.ViewModels.Streaming;
using Yavsc.Abstract.Streaming;

namespace Yavsc.Services
{
    public interface ILiveProcessor {
        /// <summary>
        /// instance keeping reference on
        /// all collections of casting and listenning websockets
        /// </summary>
        /// <value></value>
        ConcurrentDictionary<string, LiveCastHandler> Casters { get; }
        /// <summary>
        /// Try and accept websocket from aspnet http context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> AcceptStream (HttpContext context, ApplicationUser user, string destDir, string fileName);

        Task<PodcastResponse> CreateAsync(string ownerId, string ownerName, string title, CancellationToken ct);
        Task<IReadOnlyList<PodcastResponse>> ListAsync(CancellationToken ct);
        Task<PodcastManifestResponse> GetAsync(Guid id, CancellationToken ct);
        Task<PodcastResponse> AppendAsync(Guid id, string ownerId, int sequence,
            double durationSeconds, Stream input, CancellationToken ct);
        Task<PodcastResponse> CompleteAsync(Guid id, string ownerId, CancellationToken ct);
        Task<Stream> OpenSegmentAsync(Guid id, int sequence, CancellationToken ct);
        Task DeleteAsync(Guid id, string ownerId, CancellationToken ct);
    }


}
