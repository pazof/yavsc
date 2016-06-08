using System;
using System.Security.Claims;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.WebEncoders;
using OAuth.AspNet.AuthServer;
using Yavsc.Auth;
using Yavsc.Models;

namespace Yavsc
{

    public partial class Startup
    {
        private void ConfigureOAuthServices(IServiceCollection services)
        {
            services.Configure<SharedAuthenticationOptions>(options => options.SignInScheme = Constants.ExternalAuthenticationSheme);
            services.AddAuthentication(options => 
            {
                options.SignInScheme = Constants.ExternalAuthenticationSheme;
            });
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<OAuth2AppSettings>), typeof(OptionsManager<OAuth2AppSettings>)));
            // used by the YavscGoogleOAuth middelware (TODO drop it)
            services.AddTransient<Microsoft.Extensions.WebEncoders.UrlEncoder, UrlEncoder>();
            /* Obsolete:
                        var keyParamsFileInfo =
                            new FileInfo(Configuration["DataProtection:RSAParamFile"]);
                        var keyParams = (keyParamsFileInfo.Exists) ?
                            RSAKeyUtils.GetKeyParameters(keyParamsFileInfo.Name) :
                            RSAKeyUtils.GenerateKeyAndSave(keyParamsFileInfo.Name);
                        key = new RsaSecurityKey(keyParams);

                        services.Configure<TokenAuthOptions>(
                            to =>
                            {
                                to.Audience = Configuration["Site:Audience"];
                                to.Issuer = Configuration["Site:Authority"];
                                to.SigningCredentials =
                                new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature);
                            }
                        ); */
        }
        private void ConfigureOAuthApp(IApplicationBuilder app)
        {
            app.UseIdentity();
            // External authentication shared cookie:
            app.UseCookieAuthentication(options =>
            {
                options.AuthenticationScheme = Constants.ExternalAuthenticationSheme;
                options.AutomaticAuthenticate = false;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.LoginPath = new PathString(Constants.LoginPath.Substring(1));
            });

            var gvents = new OAuthEvents();
            var googleOptions = new YavscGoogleOptions
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
            googleOptions.Scope.Add("https://www.googleapis.com/auth/calendar");
            app.UseMiddleware<Yavsc.Auth.GoogleMiddleware>(googleOptions);

            // Facebook
            app.UseFacebookAuthentication(options =>
                {
                    options.AppId = Configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                    options.Scope.Add("email");
                    options.UserInformationEndpoint = "https://graph.facebook.com/v2.5/me?fields=id,name,email,first_name,last_name";
                });
            /* Generic OAuth (here GitHub): options.Notifications = new OAuthAuthenticationNotifications
                      {
                          OnGetUserInformationAsync = async context =>
                          {
                   // Get the GitHub user
                   HttpRequestMessage userRequest = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                              userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                              userRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                              HttpResponseMessage userResponse = await context.Backchannel.SendAsync(userRequest, context.HttpContext.RequestAborted);
                              userResponse.EnsureSuccessStatusCode();
                              var text = await userResponse.Content.ReadAsStringAsync();
                              JObject user = JObject.Parse(text);

                              var identity = new ClaimsIdentity(
                                  context.Options.AuthenticationType,
                                  ClaimsIdentity.DefaultNameClaimType,
                                  ClaimsIdentity.DefaultRoleClaimType);

                              JToken value;
                              var id = user.TryGetValue("id", out value) ? value.ToString() : null;
                              if (!string.IsNullOrEmpty(id))
                              {
                                  identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id, ClaimValueTypes.String, context.Options.AuthenticationType));
                              }
                              var userName = user.TryGetValue("login", out value) ? value.ToString() : null;
                              if (!string.IsNullOrEmpty(userName))
                              {
                                  identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, userName, ClaimValueTypes.String, context.Options.AuthenticationType));
                              }
                              var name = user.TryGetValue("name", out value) ? value.ToString() : null;
                              if (!string.IsNullOrEmpty(name))
                              {
                                  identity.AddClaim(new Claim("urn:github:name", name, ClaimValueTypes.String, context.Options.AuthenticationType));
                              }
                              var link = user.TryGetValue("url", out value) ? value.ToString() : null;
                              if (!string.IsNullOrEmpty(link))
                              {
                                  identity.AddClaim(new Claim("urn:github:url", link, ClaimValueTypes.String, context.Options.AuthenticationType));
                              }

                              context.Identity = identity;
                          }
                      }; */
            app.UseOAuthAuthorizationServer(

                options =>
                {
                    options.AuthorizeEndpointPath = new PathString(Constants.AuthorizePath.Substring(1));
                    options.TokenEndpointPath = new PathString(Constants.TokenPath.Substring(1));
                    options.ApplicationCanDisplayErrors = true;

#if DEBUG
                    options.AllowInsecureHttp = true;
#endif

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

                    options.AutomaticAuthenticate = false;
                }
            );
        }
    }
}
