#nullable enable

using System.Text;
using Microsoft.AspNetCore.SignalR;
using Yavsc.Abstract.Chat.Calls;

namespace Yavsc.Services;

public static class CallSignalValidator
{
    public static void Validate(CallSignal signal, bool relayOnly)
    {
        if (signal is null)
            throw new HubException("A call signal is required.");
        if (signal.Endpoint != null)
            throw new HubException("WebRTC signaling cannot carry a LAN endpoint.");
        if (signal.Kind is "offer" or "answer")
        {
            if (string.IsNullOrWhiteSpace(signal.Sdp) || Encoding.UTF8.GetByteCount(signal.Sdp) > 24000 ||
                signal.Sdp.Replace("\r\n", "").Any(c => c is '\r' or '\n') ||
                !signal.Sdp.StartsWith("v=0\r\n", StringComparison.Ordinal) ||
                !signal.Sdp.Contains("\r\nm=", StringComparison.Ordinal) ||
                signal.Candidate != null || signal.SdpMid != null || signal.SdpMLineIndex != null)
                throw new HubException("Invalid SDP signal.");
            if (relayOnly) ValidateRelaySdp(signal.Sdp);
            return;
        }
        if (signal.Kind != "candidate" || signal.Sdp != null ||
            string.IsNullOrWhiteSpace(signal.SdpMid) || signal.SdpMid.Length > 64 ||
            signal.SdpMid.Any(char.IsControl) || signal.SdpMLineIndex is not (>= 0 and <= 16) ||
            string.IsNullOrWhiteSpace(signal.Candidate) || Encoding.UTF8.GetByteCount(signal.Candidate) > 2048 ||
            signal.Candidate.Any(char.IsControl) || !signal.Candidate.StartsWith("candidate:", StringComparison.Ordinal))
            throw new HubException("Invalid ICE signal.");
        if (relayOnly && !IsRelayCandidate(signal.Candidate))
            throw new HubException("Only relay ICE candidates are allowed.");
    }

    private static bool IsRelayCandidate(string candidate)
    {
        var tokens = candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 8 || tokens[6] != "typ" || tokens[7] != "relay")
            return false;
        for (var i = 8; i < tokens.Length; i++)
            if (tokens[i] == "raddr" &&
                (i + 1 >= tokens.Length || !IsUnspecified(tokens[i + 1])))
                return false;
        return true;
    }

    private static bool IsUnspecified(string address) => address is "0.0.0.0" or "::";

    private static void ValidateRelaySdp(string sdp)
    {
        var lines = sdp.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        HashSet<string> relayAddresses = [];
        foreach (var line in lines.Where(l => l.StartsWith("a=candidate:", StringComparison.Ordinal)))
        {
            if (!IsRelayCandidate(line[2..]))
                throw new HubException("SDP contains a non-relay candidate or a related address.");
            relayAddresses.Add(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[4]);
        }
        foreach (var line in lines)
        {
            if (line.StartsWith("o=", StringComparison.Ordinal))
            {
                var address = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();
                if (!IsUnspecified(address) && address != "127.0.0.1")
                    throw new HubException("SDP origin must not expose a participant address.");
            }
            if (line.StartsWith("c=", StringComparison.Ordinal) ||
                (line.StartsWith("a=rtcp:", StringComparison.Ordinal) && line.Contains(" IN ", StringComparison.Ordinal)))
            {
                var address = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();
                if (!IsUnspecified(address) && !relayAddresses.Contains(address))
                    throw new HubException("SDP connection address is not an included relay address.");
            }
        }
    }
}
