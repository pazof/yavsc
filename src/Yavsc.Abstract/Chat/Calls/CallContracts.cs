#nullable enable

namespace Yavsc.Abstract.Chat.Calls;

public static class CallHubMethods
{
    public const string Invite = "InviteCall";
    public const string Accept = "AcceptCall";
    public const string End = "EndCall";
    public const string Signal = "SendCallSignal";
    public const string Invitation = "CallInvitation";
    public const string Accepted = "CallAccepted";
    public const string Ended = "CallEnded";
    public const string SignalReceived = "CallSignal";
}

public sealed record CallSession(
    Guid Id, string RoomName, string Caller, string Callee, bool RelayOnly,
    DateTimeOffset ExpiresAt);

public sealed record CallIceServer(string[] Urls, string? Username = null, string? Credential = null);

public sealed record CallAccepted(CallSession Session, CallIceServer[] IceServers);

public sealed record CallEnded(Guid CallId, string Reason);

public sealed record CallSignal(
    string Kind, string? Sdp = null, string? Candidate = null,
    string? SdpMid = null, int? SdpMLineIndex = null);
