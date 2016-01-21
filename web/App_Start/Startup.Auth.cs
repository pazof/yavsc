using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Yavsc.Models.Identity;
using Yavsc.Providers;
using System.Web.Configuration;
using System.Configuration;
using Yavsc.Model;
using Microsoft.Owin.Security;
using Yavsc.Helpers;

namespace Yavsc.App_Start
{
	/// <summary>
	/// Startup.
	/// </summary>
	public partial class Startup
	{
		/// <summary>
		/// Gets the O auth options.
		/// </summary>
		/// <value>The O auth options.</value>
		public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

		// For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
		public void ConfigureAuth(IAppBuilder app)
		{
			// Configure the db context and user manager to use a single instance per request
			app.CreatePerOwinContext(ApplicationDbContext.Create);
			app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

			// Enable the application to use a cookie to store information for the signed in user
			app.UseCookieAuthentication(new CookieAuthenticationOptions
				{
					AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
					LoginPath = new PathString(Paths.LoginPath),
					LogoutPath = new PathString(Paths.LogoutPath),
				});
			app.SetDefaultSignInAsAuthenticationType("External");

			// Configure the application for OAuth based flow
			YavscHelpers.PublicClientId = ConfigurationManager.AppSettings["PUBLIC_CLIENT_ID"];
			OAuthOptions = new OAuthAuthorizationServerOptions
			{
				TokenEndpointPath = new PathString(Paths.TokenPath),
				Provider = new ApplicationOAuthProvider(YavscHelpers.PublicClientId),
				AuthorizeEndpointPath = new PathString(Paths.AuthorizePath),
				AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
				AllowInsecureHttp = true
			};

			// Enable the application to use bearer tokens to authenticate users
			app.UseOAuthBearerTokens(OAuthOptions);

			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

			// Uncomment the following lines to enable logging in with third party login providers
			//app.UseMicrosoftAccountAuthentication(
			//    clientId: "",
			//    clientSecret: "");

			//app.UseTwitterAuthentication(
			//    consumerKey: "",
			//    consumerSecret: "");

			var facebookId = ConfigurationManager.AppSettings["FACEBOOK_CLIENT_ID"];
			if (!string.IsNullOrWhiteSpace(facebookId))
				app.UseFacebookAuthentication(
					new Microsoft.Owin.Security.Facebook.FacebookAuthenticationOptions() 
					{
						AppId = facebookId,
						AppSecret = ConfigurationManager.AppSettings["FACEBOOK_CLIENT_SECRET"],
						CallbackPath = new PathString(Paths.ExternalLoginPath+"Callback"),
						Scope = { "email", "public_profile" }
					});
			var googleId = ConfigurationManager.AppSettings["GOOGLE_CLIENT_ID"];
			if (!string.IsNullOrWhiteSpace(googleId))
				app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
					{
						ClientId = googleId,
						ClientSecret = ConfigurationManager.AppSettings["GOOGLE_CLIENT_SECRET"],
						CallbackPath = new PathString(Paths.ExternalLoginPath+"Callback"),
						Provider = new GoogleOAuth2AuthenticationProvider(){
							OnAuthenticated = async context =>
							{
								context.Identity.AddClaim(new Claim("picture", context.User.GetValue("picture").ToString()));
								context.Identity.AddClaim(new Claim("profile", context.User.GetValue("profile").ToString()));
							}
						},
						Scope = { "https://www.googleapis.com/auth/userinfo.email" , 
							"https://www.googleapis.com/auth/userinfo.profile" }
					});

			AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

		}
	}
}