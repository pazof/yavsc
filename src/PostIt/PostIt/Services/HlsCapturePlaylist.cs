using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PostIt.Services;

public sealed record CaptureSegment(int Sequence, double DurationSeconds, string FileName);

public static partial class HlsCapturePlaylist
{
    [GeneratedRegex(@"^segment-(\d{8})\.ts$", RegexOptions.CultureInvariant)]
    private static partial Regex SegmentName();

    public static IReadOnlyList<CaptureSegment> Parse(string playlist)
    {
        var segments = new List<CaptureSegment>();
        double? duration = null;
        foreach (var line in playlist.Split('\n'))
        {
            var text = line.Trim();
            if (text.StartsWith("#EXTINF:", StringComparison.Ordinal))
            {
                if (!double.TryParse(text[8..].Split(',')[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                    || !double.IsFinite(value) || value <= 0 || value > 15)
                    throw new FormatException("Duree du segment LibVLC invalide.");
                duration = value;
            }
            else if (text.Length > 0 && !text.StartsWith('#'))
            {
                var match = SegmentName().Match(text);
                if (!match.Success || duration is null)
                    throw new FormatException("Index HLS LibVLC invalide.");
                var sequence = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                if (segments.Count > 0 && sequence != segments[^1].Sequence + 1)
                    throw new FormatException("Segments HLS non consecutifs.");
                segments.Add(new(sequence, duration.Value, text));
                duration = null;
            }
        }
        return segments;
    }
}
