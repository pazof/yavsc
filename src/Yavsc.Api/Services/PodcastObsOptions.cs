namespace Yavsc.Api.Services;

public sealed class PodcastObsOptions
{
    public bool Enabled { get; set; }
    public string ServerUrl { get; set; } = "";
    public string RelayApiUrl { get; set; } = "http://127.0.0.1:9997/";
    public string RelayReadUrl { get; set; } = "rtmp://127.0.0.1:1935/";
    public string RelayUser { get; set; } = "yavsc";
    public string RelayPassword { get; set; } = "";
    public int ActivationMinutes { get; set; } = 30;
    public int MaxDurationMinutes { get; set; } = 120;
    public int MaxConcurrentBroadcasts { get; set; } = 2;
    public bool AllowUnencryptedRtmp { get; set; }

    public void Validate()
    {
        if (!Enabled) return;
        if (!Uri.TryCreate(ServerUrl, UriKind.Absolute, out var server)
            || (server.Scheme != "rtmps" && !(AllowUnencryptedRtmp && server.Scheme == "rtmp"))
            || server.UserInfo.Length != 0 || server.Query.Length != 0 || server.Fragment.Length != 0
            || server.AbsolutePath is not ("" or "/")
            || !Uri.TryCreate(RelayApiUrl, UriKind.Absolute, out var api) || api.Scheme is not ("http" or "https")
            || api.UserInfo.Length != 0 || api.Query.Length != 0 || api.Fragment.Length != 0
            || !Uri.TryCreate(RelayReadUrl, UriKind.Absolute, out var read) || read.Scheme != "rtmp"
            || read.UserInfo.Length != 0 || read.Query.Length != 0 || read.Fragment.Length != 0
            || read.AbsolutePath is not ("" or "/")
            || RelayPassword.Length < 32 || string.IsNullOrWhiteSpace(RelayUser)
            || ActivationMinutes is < 1 or > 1440 || MaxDurationMinutes is < 1 or > 1440
            || MaxConcurrentBroadcasts is < 1 or > 16)
            throw new InvalidOperationException("Invalid Podcast:Obs relay configuration. Use RTMPS, private relay addresses and a random password of at least 32 characters.");
    }
}
