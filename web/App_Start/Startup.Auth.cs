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
using Microsoft.Owin.Security.Facebook;

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

			// Enable the Application Sign In Cookie.
			app.UseCookieAuthentication(new CookieAuthenticationOptions
				{
					AuthenticationType = "Application",
					AuthenticationMode = AuthenticationMode.Passive,
					LoginPath = new PathString(Paths.LoginPath),
					LogoutPath = new PathString(Paths.LogoutPath),
					CookieManager = new SystemWebCookieManager()
				});

			// Enable the External Sign In Cookie.
			app.SetDefaultSignInAsAuthenticationType("External");
			app.UseExternalSignInCookie("External");

			app.UseCookieAuthentication(new CookieAuthenticationOptions
				{
					AuthenticationType = "External",
					AuthenticationMode = AuthenticationMode.Passive,
					ExpireTimeSpan = TimeSpan.FromMinutes(5),
					CookieManager = new SystemWebCookieManager()
				});

			// Enable Facebook authentication.
			var facebookId = ConfigurationManager.AppSettings["FACEBOOK_CLIENT_ID"];
			if (!string.IsNullOrWhiteSpace (facebookId)) {
				OAuthHelpers.ExternalAuthClientId.Add ("Facebook", facebookId);
				var options = new FacebookAuthenticationOptions () {
					AppId = facebookId,
					AppSecret = ConfigurationManager.AppSettings ["FACEBOOK_CLIENT_SECRET"]
				};
				app.UseFacebookAuthentication (options);
				OAuthHelpers.ExternalAuthOptions.Add("Facebook",options);
			}

			// Enable Google authentication.
			var googleId = ConfigurationManager.AppSettings["GOOGLE_CLIENT_ID"];
			if (!string.IsNullOrWhiteSpace (googleId)) {
				OAuthHelpers.ExternalAuthClientId.Add ("Google", googleId);
				var options = new GoogleOAuth2AuthenticationOptions () {
					ClientId = googleId,
					ClientSecret = ConfigurationManager.AppSettings ["GOOGLE_CLIENT_SECRET"]
				};
				OAuthHelpers.ExternalAuthOptions.Add("Google",options);
				app.UseGoogleAuthentication (options);
			}
			var authCodeProvider = new ApplicationAuthenticationTokenProvider ();
			// Setup Authorization Server
			var publicClientId = ConfigurationManager.AppSettings["PUBLIC_CLIENT_ID"];
			app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
				{
					AuthenticationType = "Application",
					AuthenticationMode = AuthenticationMode.Passive,

					AuthorizeEndpointPath = new PathString(Paths.AuthorizePath),
					TokenEndpointPath = new PathString(Paths.TokenPath),
					ApplicationCanDisplayErrors = true,
					#if DEBUG
					AllowInsecureHttp = true,
					#endif
					// Authorization server provider which controls the lifecycle of Authorization Server
					Provider = new ApplicationOAuthProvider(publicClientId),

					// Authorization code provider which creates and receives the authorization code.
					AuthorizationCodeProvider = authCodeProvider,

					// Refresh token provider which creates and receives refresh token.
					RefreshTokenProvider = authCodeProvider
				});
			var oauthprovider = new ApplicationOAuthProvider (publicClientId);

			// Configure the application for OAuth based flow
			OAuthOptions = new OAuthAuthorizationServerOptions
			{
				TokenEndpointPath = new PathString(Paths.TokenPath),
				Provider = oauthprovider,
				AuthorizeEndpointPath = new PathString(Paths.AuthorizePath),
				AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
				AllowInsecureHttp = true
			};

			// Enable the application to use bearer tokens to authenticate users
			app.UseOAuthBearerTokens(OAuthOptions);
			#region BUG
			#if BugWanted 
			// Enable the application to use a cookie to store information for the signed in user
			app.UseCookieAuthentication(new CookieAuthenticationOptions
				{
					AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
					LoginPath = new PathString(Paths.LoginPath),
					LogoutPath = new PathString(Paths.LogoutPath),
					CookieName = "Yavsc.Auth",
					TicketDataFormat =
						new SecureDataFormat<AuthenticationTicket>(DataSerializers.Ticket,
							app.GetDataProtectionProvider().Create(), TextEncodings.Base64),
					Provider = new CookieAuthenticationProvider
					{
						OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
							validateInterval: TimeSpan.FromMinutes(30),
							regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(
								manager,DefaultAuthenticationTypes.ApplicationCookie))
					},
					CookieManager = new SystemWebCookieManager()
				});

			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
			// app.SetDefaultSignInAsAuthenticationType("External");

			var oauthprovider = new ApplicationOAuthProvider (publicClientId);

			// Configure the application for OAuth based flow
			OAuthOptions = new OAuthAuthorizationServerOptions
			{
				TokenEndpointPath = new PathString(Paths.TokenPath),
				Provider = oauthprovider,
				AuthorizeEndpointPath = new PathString(Paths.AuthorizePath),
				AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
				AllowInsecureHttp = true
			};

			// Enable the application to use bearer tokens to authenticate users
			app.UseOAuthBearerTokens(OAuthOptions);

			// Uncomment the following lines to enable logging in with third party login providers
			//app.UseMicrosoftAccountAuthentication(
			//    clientId: "",
			//    clientSecret: "");

			//app.UseTwitterAuthentication(
			//    consumerKey: "",
			//    consumerSecret: "");

			#endif
			#endregion

			AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
		}
	}


}