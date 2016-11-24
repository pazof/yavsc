using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace Yavsc.ApiControllers
{
    public class PodcastController : Controller
    {
        public ArraySegment<Byte> CurrentOutput ;
        public bool IsEnd;

        ConcurrentBag<WebSocket> Listeners = new ConcurrentBag<WebSocket>();

        public async Task Connect()
        {
            var socket = await this.HttpContext.WebSockets.AcceptWebSocketAsync();
            
        }
    }
}