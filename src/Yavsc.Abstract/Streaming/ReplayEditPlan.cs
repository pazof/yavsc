namespace Yavsc.Abstract.Streaming;

/// <summary>A half-open time range [StartMs, EndMs) in the original replay timeline.</summary>
public readonly record struct ReplayRange(long StartMs, long EndMs)
{
    /// <summary>End of a kept range that runs to the end of the source.</summary>
    public const long OpenEnd = long.MaxValue;
    public long DurationMs => EndMs - StartMs;
}

/// <summary>Validation and planning of replay cuts, shared by the server and the PostIt editor.</summary>
public static class ReplayEditPlan
{
    public const int MaxRemovedRanges = 200;

    /// <summary>
    /// Converts ranges removed by the author into the ordered ranges kept in the final replay.
    /// Overlapping or touching removals are merged; removals are clamped to the source duration.
    /// </summary>
    public static IReadOnlyList<ReplayRange> KeptRanges(IEnumerable<ReplayRange> removed, long durationMs)
    {
        if (durationMs <= 0) throw new ArgumentException("La durée de la source est invalide.", nameof(durationMs));
        var cuts = removed.ToList();
        if (cuts.Count > MaxRemovedRanges)
            throw new ArgumentException($"Au plus {MaxRemovedRanges} plages peuvent être supprimées.", nameof(removed));
        foreach (var cut in cuts)
        {
            if (cut.StartMs >= cut.EndMs)
                throw new ArgumentException("Une plage supprimée doit avoir une fin postérieure à son début.", nameof(removed));
            if (cut.EndMs <= 0 || cut.StartMs >= durationMs)
                throw new ArgumentException("Une plage supprimée est en dehors de la vidéo.", nameof(removed));
        }

        var kept = new List<ReplayRange>();
        var position = 0L;
        foreach (var cut in cuts.OrderBy(c => c.StartMs))
        {
            var start = Math.Max(0, cut.StartMs);
            if (start > position) kept.Add(new ReplayRange(position, start));
            position = Math.Max(position, Math.Min(durationMs, cut.EndMs));
        }
        if (position < durationMs) kept.Add(new ReplayRange(position, durationMs));
        if (kept.Count == 0)
            throw new ArgumentException("Ces suppressions retireraient toute la vidéo.", nameof(removed));
        return kept;
    }

    /// <summary>
    /// Normalizes removals as the server applies them: clamped to the source, merged when they overlap or touch,
    /// and ordered. Throws like <see cref="KeptRanges"/> for invalid or total removals.
    /// </summary>
    public static IReadOnlyList<ReplayRange> RemovedRanges(IEnumerable<ReplayRange> removed, long durationMs)
    {
        var kept = KeptRanges(removed, durationMs);
        var result = new List<ReplayRange>();
        var position = 0L;
        foreach (var range in kept)
        {
            if (range.StartMs > position) result.Add(new ReplayRange(position, range.StartMs));
            position = range.EndMs;
        }
        if (position < durationMs) result.Add(new ReplayRange(position, durationMs));
        return result;
    }
}
