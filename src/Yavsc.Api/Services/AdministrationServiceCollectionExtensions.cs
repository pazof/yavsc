using Yavsc.ApiControllers.Administration;

namespace Yavsc.Api.Services;

public static class AdministrationServiceCollectionExtensions
{
    public static IServiceCollection AddSiteAdministration(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAdministrationService, AdministrationService>();
        services.AddScoped<AdministrationExceptionFilter>();
        services.AddOpenApi("administration", options =>
            options.ShouldInclude = description => description.GroupName == "administration");
        return services;
    }
}
