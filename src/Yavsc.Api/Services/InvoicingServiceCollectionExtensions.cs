using Yavsc.Services;

namespace Yavsc.Api.Services;

public static class InvoicingServiceCollectionExtensions
{
    public static IServiceCollection AddInvoicing(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IInvoicingService, InvoicingService>();
        services.Configure<PayPalSettings>(configuration.GetSection("Authentication:PayPal"));
        services.AddHttpClient<IPayPalTransactionVerifier, PayPalTransactionVerifier>(client =>
            client.Timeout = TimeSpan.FromSeconds(30));
        services.AddOpenApi("invoicing", options =>
            options.ShouldInclude = description => description.GroupName == "invoicing");
        return services;
    }
}
