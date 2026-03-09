
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Yavsc.Server.Helpers;

public static class ConfigurationHelpers

{
    public static string GetAuthority(this WebApplicationBuilder builder)
    {
        return builder.Configuration.GetSection("Site")
                .GetValue<string>("Authority");
    }
    public static string GetAudience(this WebApplicationBuilder builder)
    {
        return builder.Configuration.GetSection("Site")
                .GetValue<string>("Audience");
    }

}
