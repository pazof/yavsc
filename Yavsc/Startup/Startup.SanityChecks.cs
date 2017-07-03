using System;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace Yavsc
{
    public partial class Startup
    {
        public void CheckServices(IServiceCollection services)
        {
            
        }

        public void CheckApp(IApplicationBuilder app,
                SiteSettings siteSettings, IHostingEnvironment env,
                ILoggerFactory loggerFactory
                )
        {

            var logger = loggerFactory.CreateLogger<Startup>();

            var appData = Environment.GetEnvironmentVariable("APPDATA");
            if (appData == null)
            {
                if (SiteSetup.DataDir == null) {
                    SiteSetup.DataDir = "AppData";
                } else logger.LogWarning("existing setting: "+SiteSetup.DataDir);
                    DirectoryInfo di = new DirectoryInfo(SiteSetup.DataDir);
                if (!di.Exists)
                {
                    di.Create();
                    logger.LogWarning("Created dir : "+di.FullName);
                } else logger.LogWarning("existing: "+di.Name);
                SiteSetup.DataDir = Path.Combine(Directory.GetCurrentDirectory(),di.Name);
                Environment.SetEnvironmentVariable("APPDATA", SiteSetup.DataDir);
                logger.LogWarning("AppData was not found in env vars, it has been set to : "+
                Environment.GetEnvironmentVariable("APPDATA"));
            }

            var creds = GoogleSettings?.Account?.private_key;
            if (creds==null)
              throw new InvalidOperationException("No Google API credential");
            var initializer = new ServiceAccountCredential.Initializer(Startup.GoogleSettings.Account.client_email);
            initializer = initializer.FromPrivateKey(Startup.GoogleSettings.Account.private_key);
            if (initializer==null)

              throw new InvalidOperationException("Invalid Google API credential");
            
            foreach (var feature in app.ServerFeatures) 
            {
                var val = JsonConvert.SerializeObject(feature.Value);
                 logger.LogInformation( $"#Feature _{feature.Key}_: {val}" );
            }
            foreach (var prop in app.Properties) 
            {
                var val = JsonConvert.SerializeObject(prop.Value);
                 logger.LogInformation( $"#Property _{prop.Key}_: {val}" );
            }
        }
    }
}
