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

        Task<PodcastResponse> CreateAsync(string ownerId, string ownerName, string title, CancellationToken ct,
            Guid? id = null, bool refineReplay = false);
        Task<IReadOnlyList<PodcastResponse>> ListAsync(CancellationToken ct);
        Task<PodcastManifestResponse> GetAsync(Guid id, CancellationToken ct);
        Task<PodcastResponse> AppendAsync(Guid id, string ownerId, int sequence,
            double durationSeconds, Stream input, CancellationToken ct);
        Task<PodcastResponse> CompleteAsync(Guid id, string ownerId, CancellationToken ct);
        /// <summary>Opens a segment; the originals of a refined emission are reserved to its owner after the live.</summary>
        Task<Stream> OpenSegmentAsync(Guid id, int sequence, string? viewerId, CancellationToken ct);
        Task<Stream> OpenReplayAsync(Guid id, CancellationToken ct);
        Task<bool> PrepareNextReplayAsync(IPodcastReplayRemuxer remuxer, CancellationToken ct);
        Task RetryReplayAsync(Guid id, string ownerId, CancellationToken ct);
        Task<PodcastResponse> SaveDraftAsync(Guid id, string ownerId, SavePodcastDraftRequest request, CancellationToken ct);
        Task<PodcastResponse> PublishDraftAsync(Guid id, string ownerId, int revision, CancellationToken ct);
        Task<Stream> OpenDraftSourceAsync(Guid id, string ownerId, CancellationToken ct);
        Task<bool> PublishNextDraftAsync(IPodcastReplayEditor editor, CancellationToken ct);
        Task DeleteAsync(Guid id, string ownerId, CancellationToken ct);
    }


}
