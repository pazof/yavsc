using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Yavsc.ViewModels.Streaming
{
    public class LiveCastClient {
        public string UserName { get; set; }
        public WebSocket Socket { get; set; }
    }

    public class LiveEntryViewModel {
        public string UserName { get; set; }
        public string FlowId { get; set; }
    }

    public class LiveCastMeta
    {
        public WebSocket Socket { get; set; }
        public ConcurrentDictionary<string, WebSocket> Listeners { get; set; } = new ConcurrentDictionary<string, WebSocket>();
    }

}