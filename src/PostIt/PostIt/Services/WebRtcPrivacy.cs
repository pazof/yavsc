using System;
using System.Linq;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Services;

public static class WebRtcPrivacy
{
    public static void ValidateConfiguration(WebRtcMediaConfiguration configuration)
    {
        if (configuration.RelayOnly && !configuration.IceServers.Any(server =>
            server.Urls.Any(IsTurn) && !string.IsNullOrWhiteSpace(server.Username)
                && !string.IsNullOrWhiteSpace(server.Credential)))
            throw new InvalidOperationException("Masquage IP : aucun relais TURN authentifié disponible. Aucun repli direct.");
    }

    public static bool IsTurn(string url) =>
        url.StartsWith("turn:", StringComparison.Ordinal) || url.StartsWith("turns:", StringComparison.Ordinal);

    public static void ValidateSignal(CallSignal signal, bool relayOnly)
    {
        if (signal.Endpoint is not null || signal.Kind is not ("offer" or "answer" or "candidate"))
            throw new InvalidOperationException("Signal WebRTC invalide.");
        if (signal.Kind == "candidate")
        {
            if (signal.Sdp is not null || signal.Candidate is not { Length: > 0 and <= 2048 } ||
                !signal.Candidate.StartsWith("candidate:", StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(signal.SdpMid) || signal.SdpMid.Length > 256 ||
                signal.SdpMLineIndex is not (>= 0 and <= 16))
                throw new InvalidOperationException("Candidat ICE invalide.");
        }
        else if (signal.Sdp is not { Length: > 0 and <= 24576 } ||
                 !signal.Sdp.StartsWith("v=0\r\n", StringComparison.Ordinal) ||
                 signal.Candidate is not null)
            throw new InvalidOperationException("Description SDP invalide.");
        if (!relayOnly) return;
        if (signal.Candidate is { } candidate) ValidateCandidate(candidate);
        if (signal.Sdp is not { } sdp) return;
        var lines = sdp.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        var candidates = lines.Where(l => l.StartsWith("a=candidate:", StringComparison.Ordinal)).ToArray();
        foreach (var line in candidates) ValidateCandidate(line[2..]);
        var relayAddresses = candidates.Select(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries)[4]).ToHashSet();
        foreach (var line in lines)
        {
            if (line.StartsWith("o=", StringComparison.Ordinal) ||
                line.StartsWith("c=", StringComparison.Ordinal) ||
                (line.StartsWith("a=rtcp:", StringComparison.Ordinal) && line.Contains(" IN ", StringComparison.Ordinal)))
            {
                var address = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();
                if (address is not ("0.0.0.0" or "::") &&
                    !(line.StartsWith("o=", StringComparison.Ordinal) && address == "127.0.0.1") &&
                    !relayAddresses.Contains(address))
                    throw new InvalidOperationException("Masquage IP : le SDP expose une adresse non relayée.");
            }
        }
    }

    private static void ValidateCandidate(string candidate)
    {
        var parts = candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 8 || parts[6] != "typ" || parts[7] != "relay")
            throw new InvalidOperationException("Masquage IP : un candidat non relayé a été produit.");
        for (var i = 8; i < parts.Length; i++)
            if (parts[i] == "raddr" && (i + 1 >= parts.Length || parts[i + 1] is not ("0.0.0.0" or "::")))
                throw new InvalidOperationException("Masquage IP : un candidat révèle son adresse associée.");
    }

    public static void ValidateStatus(WebRtcMediaStatus status, bool relayOnly)
    {
        if (relayOnly && status.ConnectionState == "connected" && status.LocalCandidateType != "relay")
            throw new InvalidOperationException("Masquage IP : le chemin ICE sélectionné n'est pas confirmé comme relayé.");
    }
}
