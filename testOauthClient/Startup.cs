using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.Extensions.WebEncoders;
using Microsoft.AspNet.Authentication.OAuth;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace testOauthClient
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<SharedAuthenticationOptions>(options =>
            {
                options.SignInScheme = "Bearer";
            });

            services.AddTransient<Microsoft.Extensions.WebEncoders.UrlEncoder, UrlEncoder>();

            services.AddAuthentication();

            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseIISPlatformHandler(options =>
            {
                options.AuthenticationDescriptions.Clear();
            });
            app.UseStaticFiles();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                AuthenticationScheme = "Bearer",
                CookieName = CookieAuthenticationDefaults.CookiePrefix + "Bearer",
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
                LoginPath = new PathString("/signin"),
                LogoutPath = new PathString("/signout")
            });

            app.UseOAuthAuthentication(
                options =>
                {
                    options.AuthenticationScheme = "Yavsc";
                    options.AuthorizationEndpoint = "http://dev.pschneider.fr/authorize";
                    options.TokenEndpoint = "http://dev.pschneider.fr/token";
                    options.CallbackPath = new PathString("/signin-yavsc");
                    options.ClientId = "21d8bd1b-4aed-4fcb-9ed9-00b43f6a8169";
                    options.ClientSecret = "blih";
                    options.Scope.Add("profile");
                    options.SaveTokensAsClaims = true;
                    options.UserInformationEndpoint = "http://dev.pschneider.fr/api/me";
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                            {
                                var request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                                var response = await context.Backchannel.SendAsync(request);
                                response.EnsureSuccessStatusCode();

                                var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                                var identifier = payload.Value<string>("id");
                                var givenName = payload.Value<string>("givenName");
                                var emails = payload.Value<JArray>("emails");
                                string email = null;
                                if (emails !=null)
                                  email = emails.First?.Value<string>();
                                var url = payload.Value<string>("url");

                                context.Identity.AddClaim(
                                    new Claim( ClaimTypes.NameIdentifier,identifier));
                                context.Identity.AddClaim(
                                    new Claim( ClaimTypes.Name,givenName));
                                context.Identity.AddClaim(
                                    new Claim( ClaimTypes.Email,email));

                            }
                    };
                }
            );

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
    }
}
