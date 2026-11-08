using Yavsc.ApiControllers.Administration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Yavsc.Api.Services;

public static class AdministrationServiceCollectionExtensions
{
    public static IServiceCollection AddSiteAdministration(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAdministrationService, AdministrationService>();
        services.AddScoped<AdministrationExceptionFilter>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<Yavsc.Server.Helpers.IPaymentResidueService, Yavsc.Server.Helpers.PaymentResidueService>();
        services.AddOpenApi(Constants.Administration, options =>
            options.ShouldInclude = description => description.GroupName == Constants.Administration);
        return services;
    }
}
