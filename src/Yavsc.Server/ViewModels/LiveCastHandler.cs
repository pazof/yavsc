using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Yavsc.ViewModels.Streaming;

public class LiveCastClient
{
    public string UserName { get; set; }
    public WebSocket Socket { get; set; }
}

public class LiveEntryViewModel
{
    public string UserName { get; set; }
    public string FlowId { get; set; }
}

public sealed class LiveCastHandler : IDisposable
{
    private int _disposed;
    public WebSocket Socket { get; set; }
    public ConcurrentDictionary<string, WebSocket> Listeners { get; } = new();
    public CancellationTokenSource TokenSource { get; } = new();

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        TokenSource.Cancel();
        Socket?.Dispose();
        foreach (var listener in Listeners.Values) listener.Dispose();
        TokenSource.Dispose();
    }
}
