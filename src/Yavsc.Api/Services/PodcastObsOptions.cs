namespace Yavsc.Api.Services;

public sealed class PodcastObsOptions
{
    public bool Enabled { get; set; }
    public string ServerUrl { get; set; } = "";
    public string ListenAddress { get; set; } = "0.0.0.0";
    public int FirstPort { get; set; } = 10000;
    public int PortCount { get; set; } = 16;
    public int ActivationMinutes { get; set; } = 30;
    public int MaxDurationMinutes { get; set; } = 120;
    public int MaxConcurrentBroadcasts { get; set; } = 2;

    public void Validate()
    {
        if (!Enabled) return;
        if (!Uri.TryCreate(ServerUrl, UriKind.Absolute, out var server)
            || server.Scheme != "srt" || !server.IsDefaultPort
            || server.UserInfo.Length != 0 || server.Query.Length != 0 || server.Fragment.Length != 0
            || server.AbsolutePath is not ("" or "/")
            || !System.Net.IPAddress.TryParse(ListenAddress, out _)
            || FirstPort is < 1024 or > 65535 || PortCount is < 1 or > 128
            || (long)FirstPort + PortCount - 1 > 65535
            || ActivationMinutes is < 1 or > 1440 || MaxDurationMinutes is < 1 or > 1440
            || MaxConcurrentBroadcasts is < 1 or > 16 || MaxConcurrentBroadcasts > PortCount)
            throw new InvalidOperationException("Invalid Podcast:Obs configuration. Use an SRT host without a port or query, a literal listen address and a bounded UDP port range.");
    }
}
