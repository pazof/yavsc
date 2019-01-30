using System;
using System.Security.Claims;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.Facebook;
using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Authentication.Twitter;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.WebEncoders;
using OAuth.AspNet.AuthServer;
using OAuth.AspNet.Tokens;

namespace Yavsc {
    using Auth;
    using Extensions;
    using Helpers.Google;
    using Models;

    public partial class Startup {
        public static CookieAuthenticationOptions ExternalCookieAppOptions { get; private set; }

        public static IdentityOptions IdentityAppOptions { get; set; }
        public static FacebookOptions FacebookAppOptions { get; private set; }

        public static TwitterOptions TwitterAppOptions { get; private set; }
        public static OAuthAuthorizationServerOptions OAuthServerAppOptions { get; private set; }

        public static YavscGoogleOptions YavscGoogleAppOptions { get; private set; }
        public static MonoDataProtectionProvider ProtectionProvider { get; private set; }

        //  public static CookieAuthenticationOptions BearerCookieOptions { get; private set; }

        private void ConfigureOAuthServices (IServiceCollection services) 
        {
            services.Configure<SharedAuthenticationOptions> (options => options.SignInScheme = Constants.ApplicationAuthenticationSheme);

            services.Add (ServiceDescriptor.Singleton (typeof (IOptions<OAuth2AppSettings>), typeof (OptionsManager<OAuth2AppSettings>)));
            // used by the YavscGoogleOAuth middelware (TODO drop it)
            services.AddTransient<Microsoft.Extensions.WebEncoders.UrlEncoder, UrlEncoder> ();

            services.AddAuthentication (options => {
                options.SignInScheme = Constants.ExternalAuthenticationSheme;
            });

            ProtectionProvider = new MonoDataProtectionProvider (Configuration["Site:Title"]);;
            services.AddInstance<MonoDataProtectionProvider>
                (ProtectionProvider);

            services.AddIdentity<ApplicationUser, IdentityRole> (
                    option => {
                        IdentityAppOptions = option;
                        option.User.AllowedUserNameCharacters += " ";
                        option.User.RequireUniqueEmail = true;
                        // option.Cookies.ApplicationCookieAuthenticationScheme = Constants.ApplicationAuthenticationSheme;
                        option.Cookies.ApplicationCookie.LoginPath = "/signin";
                        // option.Cookies.ApplicationCookie.AuthenticationScheme = Constants.ApplicationAuthenticationSheme;
                        /*
                         option.Cookies.ApplicationCookie.DataProtectionProvider = protector;
                         option.Cookies.ApplicationCookie.LoginPath = new PathString(Constants.LoginPath.Substring(1));
                         option.Cookies.ApplicationCookie.AccessDeniedPath = new PathString(Constants.AccessDeniedPath.Substring(1));
                         option.Cookies.ApplicationCookie.AutomaticAuthenticate = true;
                         option.Cookies.ApplicationCookie.AuthenticationScheme = Constants.ApplicationAuthenticationSheme;
                         option.Cookies.ApplicationCookieAuthenticationScheme = Constants.ApplicationAuthenticationSheme;
                         option.Cookies.TwoFactorRememberMeCookie.ExpireTimeSpan = TimeSpan.FromDays(30);
                         option.Cookies.TwoFactorRememberMeCookie.DataProtectionProvider = protector;
                         option.Cookies.ExternalCookieAuthenticationScheme = Constants.ExternalAuthenticationSheme;
                         option.Cookies.ExternalCookie.AutomaticAuthenticate = true;
                         option.Cookies.ExternalCookie.AuthenticationScheme = Constants.ExternalAuthenticationSheme;
                         option.Cookies.ExternalCookie.DataProtectionProvider = protector;
                         */
                    }
                ).AddEntityFrameworkStores<ApplicationDbContext> ()
                .AddTokenProvider<EmailTokenProvider<ApplicationUser>> (Constants.DefaultFactor)
            // .AddTokenProvider<UserTokenProvider>(Constants.DefaultFactor)
            //  .AddTokenProvider<UserTokenProvider>(Constants.SMSFactor)
            //  .AddTokenProvider<UserTokenProvider>(Constants.EMailFactor)
            //  .AddTokenProvider<UserTokenProvider>(Constants.AppFactor)
            // .AddDefaultTokenProviders()
            ;
        }
        private void ConfigureOAuthApp (IApplicationBuilder app,
            SiteSettings settingsOptions, ILogger logger) {

            app.UseIdentity ();
            app.UseWhen (context => context.Request.Path.StartsWithSegments ("/api"),
                branch => {
                    branch.UseJwtBearerAuthentication (
                        options => {
                            options.AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
                            options.AutomaticAuthenticate = true;
                            options.SecurityTokenValidators.Clear ();
                            options.SecurityTokenValidators.Add (new TicketDataFormatTokenValidator (
                                ProtectionProvider
                            ));
                        }
                    );

                    // External authentication shared cookie:
                    branch.UseCookieAuthentication (options => {
                        ExternalCookieAppOptions = options;
                        options.AuthenticationScheme = Constants.ExternalAuthenticationSheme;
                        options.AutomaticAuthenticate = true;
                        options.ExpireTimeSpan = TimeSpan.FromMinutes (5);
                        options.LoginPath = new PathString (Constants.LoginPath.Substring (1));
                        // TODO implement an access denied page
                        options.AccessDeniedPath = new PathString (Constants.LoginPath.Substring (1));
                    });

                    YavscGoogleAppOptions = new YavscGoogleOptions {
                        ClientId = GoogleWebClientConfiguration["web:client_id"],
                            ClientSecret = GoogleWebClientConfiguration["web:client_secret"],
                            AccessType = "offline",
                            Scope = {
                                "profile",
                                "https://www.googleapis.com/auth/plus.login",
                                "https://www.googleapis.com/auth/admin.directory.resource.calendar",
                                "https://www.googleapis.com/auth/calendar",
                                "https://www.googleapis.com/auth/calendar.events"
                            },
                            SaveTokensAsClaims = true,
                            UserInformationEndpoint = "https://www.googleapis.com/plus/v1/people/me",
                            Events = new OAuthEvents {
                                OnCreatingTicket = async context => {
                                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory> ()
                                        .CreateScope ()) {
                                        var gcontext = context as GoogleOAuthCreatingTicketContext;
                                        context.Identity.AddClaim (new Claim (YavscClaimTypes.GoogleUserId, gcontext.GoogleUserId));
                                        var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext> ();

                                        var store = serviceScope.ServiceProvider.GetService<IDataStore> ();
                                        await store.StoreAsync (gcontext.GoogleUserId, new TokenResponse {
                                            AccessToken = gcontext.TokenResponse.AccessToken,
                                                RefreshToken = gcontext.TokenResponse.RefreshToken,
                                                TokenType = gcontext.TokenResponse.TokenType,
                                                ExpiresInSeconds = int.Parse (gcontext.TokenResponse.ExpiresIn),
                                                IssuedUtc = DateTime.Now
                                        });
                                        await dbContext.StoreTokenAsync (gcontext.GoogleUserId,
                                            gcontext.TokenResponse.Response,
                                            gcontext.TokenResponse.AccessToken,
                                            gcontext.TokenResponse.TokenType,
                                            gcontext.TokenResponse.RefreshToken,
                                            gcontext.TokenResponse.ExpiresIn);

                                    }
                                }
                            }
                    };

                    branch.UseMiddleware<Yavsc.Auth.GoogleMiddleware> (YavscGoogleAppOptions);
                    /* FIXME 403
                    
                    branch.UseTwitterAuthentication(options=>
                    {
                            TwitterAppOptions = options;
                            options.ConsumerKey = Configuration["Authentication:Twitter:ClientId"];
                            options.ConsumerSecret = Configuration["Authentication:Twitter:ClientSecret"];
                    }); */

                    branch.UseOAuthAuthorizationServer (

                        options => {
                            OAuthServerAppOptions = options;
                            options.AuthorizeEndpointPath = new PathString (Constants.AuthorizePath.Substring (1));
                            options.TokenEndpointPath = new PathString (Constants.TokenPath.Substring (1));
                            options.ApplicationCanDisplayErrors = true;
                            options.AllowInsecureHttp = true;
                            options.AuthenticationScheme = OAuthDefaults.AuthenticationType;
                            options.TokenDataProtector = ProtectionProvider.CreateProtector ("Bearer protection");

                            options.Provider = new OAuthAuthorizationServerProvider {
                                OnValidateClientRedirectUri = ValidateClientRedirectUri,
                                OnValidateClientAuthentication = ValidateClientAuthentication,
                                OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials,
                                OnGrantClientCredentials = GrantClientCredetails
                            };

                            options.AuthorizationCodeProvider = new AuthenticationTokenProvider {
                                OnCreate = CreateAuthenticationCode,
                                OnReceive = ReceiveAuthenticationCode,
                            };

                            options.RefreshTokenProvider = new AuthenticationTokenProvider {
                                OnCreate = CreateRefreshToken,
                                OnReceive = ReceiveRefreshToken,
                            };

                            options.AutomaticAuthenticate = true;
                            options.AutomaticChallenge = true;
                        }
                    );
                });

            Environment.SetEnvironmentVariable ("GOOGLE_APPLICATION_CREDENTIALS", "google-secret.json");

        }
    }
}