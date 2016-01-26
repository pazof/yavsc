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
using Microsoft.Owin.Security.DataProtection;
using Yavsc.Model;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System.Collections.Concurrent;
using System.Web.Cors;
using System.Threading.Tasks;
using System.Net;
using Yavsc.Model.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataHandler.Serializer;
using Microsoft.Owin.Security.DataHandler.Encoder;

namespace SignalRSelfHost
{
    
    class Startup
    {
        public void Configuration(IAppBuilder app)
		{
			// Configure the db context and user manager to use a single instance per request
			app.CreatePerOwinContext(ApplicationDbContext.Create);
			app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

			app.UseCors(new CorsOptions
				{
					PolicyProvider = new CorsPolicyProvider
					{
						PolicyResolver = context =>
						{
							var policy = new CorsPolicy();
							policy.AllowAnyOrigin = true;
							policy.AllowAnyMethod = true;
							policy.AllowAnyHeader = true;
							policy.SupportsCredentials = false;
							return Task.FromResult(policy);
						}
					}
				}
			);


			app.MapSignalR(new HubConfiguration
				{
					EnableDetailedErrors = true,
					EnableJSONP = true
				}
				
			);
			ConfigureSecurity (app);
			ConfigureAuth(app);
			var webApiConfiguration = ConfigureWebApi();
			app.UseWebApi(webApiConfiguration);

			#if DEBUG
			app.UseErrorPage();
			#endif
			app.UseWelcomePage("/");
        }

		public void ConfigureSecurity(IAppBuilder app)
		{
			app.SetDataProtectionProvider(new MonoDataProtectionProvider(app.Properties["host.AppName"] as string));
		}

		private void ConfigureAuth(IAppBuilder app)

		{


			// Enable the application to use a cookie to store information for the signed in user
			app.UseCookieAuthentication(new CookieAuthenticationOptions
				{
					AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
					LoginPath = new PathString(Paths.LoginPath),
					LogoutPath = new PathString(Paths.LogoutPath),
					CookieName = "Yavsc.Auth",
					TicketDataFormat =
						new SecureDataFormat<AuthenticationTicket>(DataSerializers.Ticket,
							app.GetDataProtectionProvider().Create(), TextEncodings.Base64)
						
				});
			
			app.SetDefaultSignInAsAuthenticationType("External");

			
			var OAuthOptions = new OAuthAuthorizationServerOptions
			{
				AuthorizeEndpointPath = new PathString(Paths.AuthorizePath),

				TokenEndpointPath = new PathString(Paths.TokenPath),
				Provider = new YavscOAuthServerProvider(),
				AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
				ApplicationCanDisplayErrors = true,
				#if DEBUG
				// Only do this for demo!!
				AllowInsecureHttp = true,
				#endif
					// Authorization code provider which creates and receives the authorization code.
					AuthorizationCodeProvider = new AuthenticationTokenProvider
				{
					OnCreate = CreateAuthenticationCode,
					OnReceive = ReceiveAuthenticationCode,
				},

				// Refresh token provider which creates and receives refresh token.
				RefreshTokenProvider = new AuthenticationTokenProvider
				{
					OnCreate = CreateRefreshToken,
					OnReceive = ReceiveRefreshToken,
				}
					
			};
			app.UseOAuthAuthorizationServer(OAuthOptions);     
			app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
			{Provider = new ApplicationOAuthBearerAuthenticationProvider()});
			
			//app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
		}


		private HttpConfiguration ConfigureWebApi()
		{
			var config = new HttpConfiguration();
			config.Routes.MapHttpRoute(
				"DefaultApi",
				"api/{controller}/{id}",
				new { id = RouteParameter.Optional });

			config.MapHttpAttributeRoutes();
			return config;
		}


		private readonly ConcurrentDictionary<string, string> _authenticationCodes =
			new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

		private void CreateAuthenticationCode(AuthenticationTokenCreateContext context)
		{
			context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
			_authenticationCodes[context.Token] = context.SerializeTicket();
		}

		private void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
		{
			string value;
			if (_authenticationCodes.TryRemove(context.Token, out value))
			{
				context.DeserializeTicket(value);
			}
		}

		private void CreateRefreshToken(AuthenticationTokenCreateContext context)
		{
			context.SetToken(context.SerializeTicket());
		}

		private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
		{
			context.DeserializeTicket(context.Token);
		}
	}
    
}