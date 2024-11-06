using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Yavsc.Helpers;
using Yavsc.Settings;

namespace Yavsc;

public  class Startup
{
      public static void Configure(
            IApplicationBuilder app,
            IOptions<SiteSettings> siteSettings,
             IOptions<SmtpSettings> smtpSettings,
        IAuthorizationService authorizationService,
        IOptions<PayPalSettings> payPalSettings,
        IOptions<GoogleAuthSettings> googleSettings,
        IStringLocalizer<YavscLocalization> localizer,
         ILoggerFactory loggerFactory,
         string environmentName)
        {
            Config.GoogleSettings = googleSettings.Value;
            ResourcesHelpers.GlobalLocalizer = localizer;
            Config.SiteSetup = siteSettings.Value;
            Config.SmtpSetup = smtpSettings.Value;
            Config.Authority = siteSettings.Value.Authority;
            string blogsDir = siteSettings.Value.Blog ?? throw new Exception("blogsDir is not set.");
            string billsDir = siteSettings.Value.Bills ?? throw new Exception("billsDir is not set.");
            AbstractFileSystemHelpers.UserFilesDirName = new DirectoryInfo(blogsDir).FullName;
            AbstractFileSystemHelpers.UserBillsDirName = new DirectoryInfo(billsDir).FullName;
            Config.Temp = siteSettings.Value.TempDir;
            Config.PayPalSettings = payPalSettings.Value;

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // TODO implement an installation & upgrade procedure
            // Create required directories
            foreach (string dir in new string[] { AbstractFileSystemHelpers.UserFilesDirName, AbstractFileSystemHelpers.UserBillsDirName, Config.Temp })
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
            CheckApp(loggerFactory.CreateLogger<Startup>(), environmentName);
        }

        public static void CheckApp(ILogger logger, string environmentName)
        {
            var appData = Environment.GetEnvironmentVariable("APPDATA");
            if (appData == null)
            {
                logger.LogWarning("AppData was not found in environment variables");
                if (Config.SiteSetup.DataDir == null) {
                    Config.SiteSetup.DataDir = "AppData"+environmentName;
                    logger.LogInformation("Using: "+Config.SiteSetup.DataDir);
                } else logger.LogInformation("Using value from settings: "+Config.SiteSetup.DataDir);
                DirectoryInfo di = new DirectoryInfo(Config.SiteSetup.DataDir);
                if (!di.Exists)
                {
                    di.Create();
                    logger.LogWarning("Created dir : "+di.FullName);
                }
                logger.LogInformation("Using existing directory: "+di.FullName);
                Environment.SetEnvironmentVariable("APPDATA", Config.SiteSetup.DataDir);
                logger.LogWarning("It has been set to : "+Environment.GetEnvironmentVariable("APPDATA"));
            }

        }
}
