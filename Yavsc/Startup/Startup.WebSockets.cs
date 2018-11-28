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
        }
    }
}