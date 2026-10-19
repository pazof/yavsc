namespace Yavsc.Services;

public sealed class PodcastOptions
{
    public string RootPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "DataDir", "podcasts");
    public int MaxSegmentBytes { get; set; } = 8 * 1024 * 1024;
    public long MaxOwnerBytes { get; set; } = 1024L * 1024 * 1024;
    public int MaxSegments { get; set; } = 7200;
    public int MaxOwnerPodcasts { get; set; } = 100;
    public int IdleTimeoutSeconds { get; set; } = 60;
}
