using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;

namespace Yavsc
{
    public partial class Startup
    {
        public void ConfigureWebSocketsApp(IApplicationBuilder app,
                SiteSettings siteSettings, IHostingEnvironment env)
        {
            app.UseWebSockets();

            app.UseSignalR("/api/signalr");
/*
            var _sockets = new ConcurrentBag<WebSocket>();

            app.Use(
               async (http, next) =>
               {
                   if (http.WebSockets.IsWebSocketRequest)
                   {
                       WebSocket webSocket = null;
                       if (!_sockets.TryPeek(out webSocket))
                       {
                           webSocket = await http.WebSockets.AcceptWebSocketAsync();
                           _sockets.Add(webSocket);
                       }
                       using (webSocket)
                       {
                           if (webSocket != null && webSocket.State == WebSocketState.Open)
                           {
                               // TODO: Handle the socket here.
                               byte[] buffer = new byte[1024];
                               WebSocketReceiveResult received = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                               while (received.MessageType != WebSocketMessageType.Close)
                               {
                                   Console.WriteLine($"Echoing {received.Count} bytes received in a {received.MessageType} message; Fin={received.EndOfMessage}");
                                   // Echo anything we receive
                                   await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, received.Count), received.MessageType, received.EndOfMessage, CancellationToken.None);

                                   received = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                               }
                               await webSocket.CloseAsync(received.CloseStatus.Value, received.CloseStatusDescription, CancellationToken.None);

                           }
                           
                       }
                   }
                   else
                   {
                       // Nothing to do here, pass downstream.  
                       await next();
                   }
               }
            ); */
        }
    }
}