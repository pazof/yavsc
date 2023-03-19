
using Microsoft.AspNetCore.Hosting;

namespace Yavsc
{
    // ensures we may count on :
    // * Google credentials
    // * an AppData folder
    public partial class Startup
    {

        public void CheckApp(Microsoft.AspNetCore.Hosting.IHostingEnvironment env,
                ILoggerFactory loggerFactory)
        {

            var logger = loggerFactory.CreateLogger<Startup>();

            var appData = Environment.GetEnvironmentVariable("APPDATA");
            if (appData == null)
            {
                logger.LogWarning("AppData was not found in environment variables");
                if (SiteSetup.DataDir == null) {
                    SiteSetup.DataDir = "AppData"+env.EnvironmentName;
                    logger.LogInformation("Using: "+SiteSetup.DataDir);
                } else logger.LogInformation("Using value from settings: "+SiteSetup.DataDir);
                    DirectoryInfo di = new DirectoryInfo(SiteSetup.DataDir);
                if (!di.Exists)
                {
                    di.Create();
                    logger.LogWarning("Created dir : "+di.FullName);
                }
                else logger.LogInformation("Using existing directory: "+di.Name);
                SiteSetup.DataDir = Path.Combine(Directory.GetCurrentDirectory(),di.Name);
                Environment.SetEnvironmentVariable("APPDATA", SiteSetup.DataDir);
                logger.LogWarning("It has been set to : "+Environment.GetEnvironmentVariable("APPDATA"));
            }

        }
    }
}
