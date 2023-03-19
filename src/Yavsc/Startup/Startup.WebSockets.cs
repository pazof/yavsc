namespace Yavsc
{
    public partial class Startup
    {
        public void ConfigureWebSocketsApp(IApplicationBuilder app)
        {
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(30),
                ReceiveBufferSize = Constants.WebSocketsMaxBufLen
            };
            
            app.UseWebSockets(webSocketOptions);
        }

    }
}
