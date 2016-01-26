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

			// Enable the Application Sign In Cookie.
			app.UseCookieAuthentication(new CookieAuthenticationOptions
				{
					AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
					LoginPath = new PathString(Paths.LoginPath),
					LogoutPath = new PathString(Paths.LogoutPath)
				});

			app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ExternalCookie);
			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

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


			//app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ExternalCookie);

			// Configure the application for OAuth based flow
			/*
			 * 
			 * YavscHelpers.PublicClientId = ConfigurationManager.AppSettings["PUBLIC_CLIENT_ID"];
			OAuthOptions = new OAuthAuthorizationServerOptions
			{
				TokenEndpointPath = new PathString(Paths.TokenPath),
				Provider = new ApplicationOAuthProvider(YavscHelpers.PublicClientId),
				AuthorizeEndpointPath = new PathString(Paths.AuthorizePath),
				AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
				AllowInsecureHttp = true
			};
*/
			// Enable the application to use bearer tokens to authenticate users
			// app.UseOAuthBearerTokens(OAuthOptions);



			if (false) { 
				// Setup Authorization Server
				var authCodeProvider = new ApplicationAuthenticationTokenProvider ();
				var publicClientId = ConfigurationManager.AppSettings["PUBLIC_CLIENT_ID"];
				app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
					{
						AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
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
			}
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
	/*
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

	*/

}