using System;
using System.Security.Claims;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.Facebook;
using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.WebEncoders;
using OAuth.AspNet.AuthServer;
using OAuth.AspNet.Tokens;
using Yavsc.Auth;
using Yavsc.Extensions;
using Yavsc.Models;

namespace Yavsc
{

    public partial class Startup
    {
        public static CookieAuthenticationOptions ExternalCookieAppOptions { get; private set; }

        public static IdentityOptions IdentityAppOptions { get; set; }
        public static FacebookOptions FacebookAppOptions { get; private set; }
        public static OAuthAuthorizationServerOptions OAuthServerAppOptions { get; private set; }

        public static OAuthOptions OAuthClientOptions { get; set; }

        public static YavscGoogleOptions YavscGoogleAppOptions { get; private set; }
        public static MonoDataProtectionProvider ProtectionProvider { get; private set; }

        //  public static CookieAuthenticationOptions BearerCookieOptions { get; private set; }

        private void ConfigureOAuthServices(IServiceCollection services)
        {
            services.Configure<SharedAuthenticationOptions>(options => options.SignInScheme = Constants.ApplicationAuthenticationSheme);

            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<OAuth2AppSettings>), typeof(OptionsManager<OAuth2AppSettings>)));
            // used by the YavscGoogleOAuth middelware (TODO drop it)
            services.AddTransient<Microsoft.Extensions.WebEncoders.UrlEncoder, UrlEncoder>();
            
            services.AddAuthentication(options =>
            {
                options.SignInScheme = Constants.ExternalAuthenticationSheme;
            });

            ProtectionProvider = new MonoDataProtectionProvider(Configuration["Site:Title"]); ;
            services.AddInstance<MonoDataProtectionProvider>
            (ProtectionProvider);

            services.AddIdentity<ApplicationUser, IdentityRole>(
                option =>
                {
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
            ).AddEntityFrameworkStores<ApplicationDbContext>()
                 .AddTokenProvider<EmailTokenProvider<ApplicationUser>>(Constants.EMailFactor)
                 .AddTokenProvider<UserTokenProvider>(Constants.DefaultFactor)
                ;

        }
        private void ConfigureOAuthApp(IApplicationBuilder app, SiteSettings settings)
        {
            app.UseIdentity();
            app.UseOAuthAuthorizationServer(

                       options =>
                       {
                           OAuthServerAppOptions = options;
                           options.AuthorizeEndpointPath = new PathString(Constants.AuthorizePath.Substring(1));
                           options.TokenEndpointPath = new PathString(Constants.TokenPath.Substring(1));
                           options.ApplicationCanDisplayErrors = true;
                           options.AllowInsecureHttp = true;
                           options.AuthenticationScheme = OAuthDefaults.AuthenticationType;
                           options.TokenDataProtector = ProtectionProvider.CreateProtector("Bearer protection");

                           options.Provider = new OAuthAuthorizationServerProvider
                           {
                               OnValidateClientRedirectUri = ValidateClientRedirectUri,
                               OnValidateClientAuthentication = ValidateClientAuthentication,
                               OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials,
                               OnGrantClientCredentials = GrantClientCredetails
                           };

                           options.AuthorizationCodeProvider = new AuthenticationTokenProvider
                           {
                               OnCreate = CreateAuthenticationCode,
                               OnReceive = ReceiveAuthenticationCode,
                           };

                           options.RefreshTokenProvider = new AuthenticationTokenProvider
                           {
                               OnCreate = CreateRefreshToken,
                               OnReceive = ReceiveRefreshToken,
                           };

                           options.AutomaticAuthenticate = true;
                           options.AutomaticChallenge = true;
                       }
                   );

            app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"),
               branch =>
               {
                   branch.UseJwtBearerAuthentication(
                                                options =>
                                                {
                                                    options.AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
                                                    options.AutomaticAuthenticate = true;
                                                    options.SecurityTokenValidators.Clear();
                                                    options.SecurityTokenValidators.Add(new TicketDataFormatTokenValidator(
                                                        ProtectionProvider
                                                    ));
                                                }
                                            );

               });
            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"),
               branch =>
               {
                   // External authentication shared cookie:
                   branch.UseCookieAuthentication(options =>
                   {
                       ExternalCookieAppOptions = options;
                       options.AuthenticationScheme = Constants.ExternalAuthenticationSheme;
                       options.AutomaticAuthenticate = true;
                       options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                       options.LoginPath = new PathString(Constants.LoginPath.Substring(1));
                       // TODO implement an access denied page
                       options.AccessDeniedPath = new PathString(Constants.LoginPath.Substring(1));
                   });

                  

                   var gvents = new OAuthEvents();
                   YavscGoogleAppOptions = new YavscGoogleOptions
                   {
                       ClientId = Configuration["Authentication:Google:ClientId"],
                       ClientSecret = Configuration["Authentication:Google:ClientSecret"],
                       AccessType = "offline",
                       SaveTokensAsClaims = true,
                       UserInformationEndpoint = "https://www.googleapis.com/plus/v1/people/me",
                       Events = new OAuthEvents
                       {
                           OnCreatingTicket = async context =>
                           {
                               using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                                       .CreateScope())
                               {
                                   var gcontext = context as GoogleOAuthCreatingTicketContext;
                                   context.Identity.AddClaim(new Claim(YavscClaimTypes.GoogleUserId, gcontext.GoogleUserId));
                                   var service =
                                   serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                                   await service.StoreTokenAsync(gcontext.GoogleUserId, context.TokenResponse);
                               }
                           }
                       }
                   };
                   YavscGoogleAppOptions.Scope.Add("https://www.googleapis.com/auth/calendar");
                   branch.UseMiddleware<Yavsc.Auth.GoogleMiddleware>(YavscGoogleAppOptions);

                   // Facebook
                   branch.UseFacebookAuthentication(options =>
                       {
                           FacebookAppOptions = options;
                           options.AppId = Configuration["Authentication:Facebook:AppId"];
                           options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                           options.Scope.Add("email");
                           options.UserInformationEndpoint = "https://graph.facebook.com/v2.5/me?fields=id,name,email,first_name,last_name";
                       });
               });




        }
    }
}
