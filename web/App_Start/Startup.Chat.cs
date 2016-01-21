
using Owin; 
using Microsoft.Owin;
using Microsoft.AspNet.SignalR;

 
namespace Yavsc.App_Start 
{ 
	public partial class Startup 
	{ 
		public void ConfigureChat(IAppBuilder app) 
		{ 
			// Any connection or hub wire up and configuration should go here 

			// app.MapSignalR(); 

			app.MapSignalR<MyConnection>("/signalr",new HubConfiguration()
				{
					EnableDetailedErrors=true,
					EnableJavaScriptProxies=true
				}
			);

		} 
	} 
}
