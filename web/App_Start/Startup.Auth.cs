using Microsoft.Owin.Security.OAuth;
using Owin;
using Yavsc.Model.Identity;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security;
using Microsoft.Owin;
using Yavsc.Model;
using System;
using Microsoft.Owin.Security.Infrastructure;
using Yavsc.Providers;
using System.Configuration;
using System.Web.Helpers;
using System.Security.Claims;
using Yavsc.Helpers.OAuth;
using Microsoft.Owin.Security.Google;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Facebook;
using Yavsc.Helpers;
using Yavsc.Model.Authentication;

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
		// For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
		public void ConfigureAuth(IAppBuilder app)
		{
			app.CreatePerOwinContext(ApplicationDbContext.Create);
			app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

			// Enable the application to use a cookie to store information for the signed in user
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
				CookieHttpOnly = true,
				LoginPath = new PathString(Paths.LoginPath),
				LogoutPath = new PathString(Paths.LogoutPath),
					CookieName= "Yavsc.Auth"
			});
	
			//app.SetDefaultSignInAsAuthenticationType("Application");

			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

			//app.UseMicrosoftAccountAuthentication(
			//    clientId: "",
			//    clientSecret: "");

			//app.UseTwitterAuthentication(
			//    consumerKey: "",
			//    consumerSecret: "");
			// Enable Facebook authentication.

			var facebookId = ConfigurationManager.AppSettings ["FACEBOOK_CLIENT_ID"];
			if (!string.IsNullOrWhiteSpace (facebookId)) {
				OAuthHelpers.ExternalAuthClientId.Add ("Facebook", facebookId);
				var options = new FacebookAuthenticationOptions () {
					AppId = facebookId,
					AppSecret = ConfigurationManager.AppSettings ["FACEBOOK_CLIENT_SECRET"],
					CallbackPath = new PathString("/OAuth/ExternalCallback/FaceBook")
				};
				app.UseFacebookAuthentication (options);
				OAuthHelpers.ExternalAuthOptions.Add ("Facebook", options);
			}

			// Enable Google authentication.
			var googleId = ConfigurationManager.AppSettings["GOOGLE_CLIENT_ID"];
			var googleSecret = ConfigurationManager.AppSettings["GOOGLE_CLIENT_SECRET"];
			if (!string.IsNullOrWhiteSpace (googleId)) {
				OAuthHelpers.ExternalAuthClientId.Add ("Google", googleId);
				var options = new GoogleOAuth2AuthenticationOptions()
				{
					ClientId = googleId,
					ClientSecret = googleSecret,
					CallbackPath = new PathString("/OAuth/ExternalCallback/Google")
				};
				OAuthHelpers.ExternalAuthOptions.Add("Google",options);
				app.UseGoogleAuthentication (options);
			}
				
			// AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
		}
	}

}
