#nullable enable

namespace Yavsc.Abstract.Chat.Calls;

public static class CallHubMethods
{
    public const string Invite = "InviteCall";
    public const string InviteLan = "InviteLanCall";
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
    DateTimeOffset ExpiresAt, string Transport = CallTransports.WebRtc);

public static class CallTransports
{
    public const string WebRtc = "webrtc";
    public const string LanSrtp = "lan-srtp";
}

public sealed record CallIceServer(string[] Urls, string? Username = null, string? Credential = null);

public sealed record CallAccepted(CallSession Session, CallIceServer[] IceServers);

public sealed record CallEnded(Guid CallId, string Reason);

public sealed record CallSignal(
    string Kind, string? Sdp = null, string? Candidate = null,
    string? SdpMid = null, int? SdpMLineIndex = null, LanCallEndpoint? Endpoint = null);

public sealed record LanCallEndpoint(string Address, int Port, string Key, string Salt)
{
    public override string ToString() => "SRTP endpoint (credentials redacted)";

    public bool IsValid() =>
        LanCallNetwork.IsPrivateIPv4(Address) && Port is >= 1024 and <= 65534 && Port % 2 == 0 &&
        Key is { Length: 32 } && Key.All(Uri.IsHexDigit) &&
        Salt is { Length: 28 } && Salt.All(Uri.IsHexDigit);
}

public static class LanCallNetwork
{
    public static bool IsPrivateIPv4(string address)
    {
        if (!System.Net.IPAddress.TryParse(address, out var ip) ||
            ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
            ip.ToString() != address)
            return false;
        var bytes = ip.GetAddressBytes();
        return bytes[0] == 10 || bytes[0] == 127 ||
            (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) ||
            (bytes[0] == 192 && bytes[1] == 168);
    }
}
