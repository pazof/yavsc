#nullable enable

namespace Yavsc.Settings;

public sealed class AudioVideoCallOptions
{
    public const string SectionName = "AudioVideoCalls";

    public bool Enabled { get; set; }
    public string[] StunUrls { get; set; } = [];
    public string[] TurnUrls { get; set; } = [];
    public string? TurnSharedSecret { get; set; }
}
