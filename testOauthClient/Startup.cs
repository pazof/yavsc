using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Authentication.Cookies;

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
            
            services.Configure<SharedAuthenticationOptions>(options => {
                options.SignInScheme = "ClientCookie";
            });

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
             app.UseIISPlatformHandler(options => {
                options.AuthenticationDescriptions.Clear();
            });
            app.UseStaticFiles();
             app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                AuthenticationScheme = "ClientCookie",
                CookieName = CookieAuthenticationDefaults.CookiePrefix + "ClientCookie",
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
                LoginPath = new PathString("/signin"),
                LogoutPath = new PathString("/signout")
            });
            
          

            app.UseOpenIdConnectAuthentication(
                options => {
                options.AuthenticationScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.RequireHttpsMetadata = false;

                // Note: these settings must match the application details
                // inserted in the database at the server level.
                options.ClientId = "016c5ae4-f4cd-40e3-b250-13701c871ecd";
                options.ClientSecret = "blahblah";
                options.PostLogoutRedirectUri = "http://dev.pschneider.fr/";

                // Use the authorization code flow.
                options.ResponseType = OpenIdConnectResponseTypes.Code;

                // Note: setting the Authority allows the OIDC client middleware to automatically
                // retrieve the identity provider's configuration and spare you from setting
                // the different endpoints URIs or the token validation parameters explicitly.
                options.Authority = "http://dev.pschneider.fr/";

                // Note: the resource property represents the different endpoints the
                // access token should be issued for (values must be space-delimited).
                options.Resource = "http://dev.pschneider.fr/";
               //  options.Scope.Add("api-resource-controller");
            });


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
