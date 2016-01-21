using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using System.Configuration;

namespace SignalRSelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.
			string url = ConfigurationManager.AppSettings["Url"];
            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                
				while (true) {
					System.Threading.Thread.Sleep (50000);
				}
            }
        }
    }
}