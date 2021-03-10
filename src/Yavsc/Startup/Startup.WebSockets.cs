using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.WebSockets.Server;

namespace Yavsc
{
    public partial class Startup
    {
        public void ConfigureWebSocketsApp(IApplicationBuilder app)
        {
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(30),
                ReceiveBufferSize = Constants.WebSocketsMaxBufLen,
                ReplaceFeature = false
            };
            
            app.UseWebSockets(webSocketOptions);
            app.UseSignalR(PathString.FromUriComponent(Constants.SignalRPath));
        }

    }
}
