


using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Yavsc.Server.Helpers;
using Yavsc.Settings;

namespace Yavsc;
public  class Startup
{
      public static void Configure(
            IApplicationBuilder app,
            IOptions<SiteSettings> siteSettings,
             IOptions<SmtpSettings> smtpSettings,
        IOptions<PayPalSettings> payPalSettings,
        IOptions<GoogleAuthSettings> googleSettings,
        IStringLocalizer<Startup> localizer,
         ILoggerFactory loggerFactory,
         string environmentName)
        {
            Config.GoogleSettings = googleSettings.Value;
            ResourcesHelpers.GlobalLocalizer = localizer;
            Config.SmtpSetup = smtpSettings.Value;
            Config.Authority = siteSettings.Value.Authority;
            string blogsDir = siteSettings.Value.Blog ?? throw new Exception("blogsDir is not set.");
            string billsDir = siteSettings.Value.Bills ?? throw new Exception("billsDir is not set.");
            string tempDir = siteSettings.Value.TempDir ?? throw new Exception("tempDir is not set.");
           
            Config.PayPalSettings = payPalSettings.Value;

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // TODO implement an installation & upgrade procedure
            // Create required directories
            foreach (string dir in new string[] { blogsDir, billsDir, tempDir })
            {
                if (dir == null)
                {
                    throw new Exception(nameof(dir));
                }

                DirectoryInfo di = new(dir);
                if (!di.Exists)
                {
                    di.Create();
                }
            }
            CheckApp(siteSettings.Value, loggerFactory.CreateLogger<Startup>(), environmentName);
        }

        public static void CheckApp(SiteSettings settings, ILogger logger, string environmentName)
        {
            var appData = Environment.GetEnvironmentVariable("APPDATA");
            if (appData == null)
            {
                logger.LogWarning("AppData was not found in environment variables");
                if (settings.DataDir == null) {
                    settings.DataDir = "AppData"+environmentName;
                    logger.LogInformation("Using: "+settings.DataDir);
                } else logger.LogInformation("Using value from settings: "+settings.DataDir);
                DirectoryInfo di = new DirectoryInfo(settings.DataDir);
                if (!di.Exists)
                {
                    di.Create();
                    logger.LogWarning("Created dir : "+di.FullName);
                }
                logger.LogInformation("Using existing directory: "+di.FullName);
                Environment.SetEnvironmentVariable("APPDATA", settings.DataDir);
                logger.LogWarning("It has been set to : "+Environment.GetEnvironmentVariable("APPDATA"));
            }

        }
}
