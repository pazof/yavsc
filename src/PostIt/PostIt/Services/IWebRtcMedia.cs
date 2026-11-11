using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Services;

public sealed record WebRtcMediaConfiguration(CallIceServer[] IceServers, bool RelayOnly,
    bool Caller, string Camera, string Microphone);

public sealed record WebRtcMediaStatus(string ConnectionState, string? LocalCandidateType,
    long AudioPackets, long VideoFrames, bool LocalVideoRendered = false, bool RemoteVideoRendered = false)
{
    public bool HasRemoteMedia => ConnectionState == "connected" && AudioPackets > 0 && VideoFrames > 0;
}

public interface IWebRtcMedia : IAsyncDisposable
{
    Control LocalVideo { get; }
    Control RemoteVideo { get; }
    event Action<CallSignal>? Signal;
    event Action<WebRtcMediaStatus>? StatusChanged;
    Task Completion { get; }
    Task StartAsync(WebRtcMediaConfiguration configuration, CancellationToken ct);
    Task HandleSignalAsync(CallSignal signal, CancellationToken ct);
}

public interface IWebRtcCallSignaling : ILanCallSignaling
{
    Task<CallSession> InviteAsync(string room, string user, CancellationToken ct = default);
}
