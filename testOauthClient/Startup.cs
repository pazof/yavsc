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
            var host = "http://dev.pschneider.fr";
            app.UseOAuthAuthentication(
                options =>
                {
                    options.AuthenticationScheme = "Yavsc";
                    options.AuthorizationEndpoint = $"{host}/authorize";
                    options.TokenEndpoint = $"{host}/dev.pschneider.fr/token";
                    options.CallbackPath = new PathString("/signin-yavsc");
                    options.ClientId = "[Your client Id]";
                    options.ClientSecret = "blih";
                    options.Scope.Add("profile");
                    options.SaveTokensAsClaims = true;
                    options.UserInformationEndpoint = $"{host}/api/me";
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                            {
                                var request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                                var response = await context.Backchannel.SendAsync(request);
                                response.EnsureSuccessStatusCode();

                                var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                                var identifier = payload.Value<string>("Id");
                                var givenName = payload.Value<string>("UserName");
                                var emails = payload.Value<JArray>("EMails");
                                var roles = payload.Value<JArray>("Roles");
                                string email = null;
                                if (emails !=null)
                                  email = emails.First?.Value<string>();
                                context.Identity.AddClaim(
                                    new Claim( ClaimTypes.NameIdentifier,identifier));
                                context.Identity.AddClaim(
                                    new Claim( ClaimTypes.Name,givenName));
                                context.Identity.AddClaim(
                                    new Claim( ClaimTypes.Email,email));
                                    // TODO add all emails and roles

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
