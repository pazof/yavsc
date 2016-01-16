//
//  Startup.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016 GNU GPL
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using System.Configuration;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin;
using System.Web.Http;

namespace SignalRSelfHost
{
    
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
			ConfigureAuth(app);
			var webApiConfiguration = ConfigureWebApi();
			app.UseWebApi(webApiConfiguration);

        }
		private void ConfigureAuth(IAppBuilder app)
		{
			var OAuthOptions = new OAuthAuthorizationServerOptions
			{
				TokenEndpointPath = new PathString("/Token"),
				Provider = new YavscOAuthServerProvider(),
				AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),

				// Only do this for demo!!
				AllowInsecureHttp = true
			};
			app.UseOAuthAuthorizationServer(OAuthOptions);               
			app.UseOAuthBearerAuthentication(
				new OAuthBearerAuthenticationOptions());
		}


		private HttpConfiguration ConfigureWebApi()
		{
			var config = new HttpConfiguration();
			config.Routes.MapHttpRoute(
				"DefaultApi",
				"api/{controller}/{id}",
				new { id = RouteParameter.Optional });
			return config;
		}
    }
    
}