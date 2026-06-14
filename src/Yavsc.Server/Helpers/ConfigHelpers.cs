using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Yavsc.Server.Helpers;

public static class ConfigHelpers
{
    public static IConfigurationBuilder AddConfiguration(this WebApplicationBuilder builder, string appName )
        {
            var rootConfig = builder.Configuration
                            .AddJsonFile($"appsettings-{appName}.json", optional: false, reloadOnChange: false);

            if (!String.IsNullOrWhiteSpace(builder.Environment.EnvironmentName))
                rootConfig.AddJsonFile($"appsettings-{appName}.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
            ;
            return rootConfig.AddEnvironmentVariables();
        }
}