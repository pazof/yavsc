
using Anthropic.SDK;
using Microsoft.AspNetCore;
using Yavsc.Abstract.Interfaces;
using Yavsc.Extensions;

namespace Yavsc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Anthropic client
            builder.Services.AddSingleton<AnthropicClient>(_ =>
                new AnthropicClient(
                    new APIAuthentication(
                        builder.Configuration["ANTHROPIC_API_KEY"]
                            ?? throw new InvalidOperationException("ANTHROPIC_API_KEY manquante")
                    )
                )
            );

            // Service de modération
            if (builder.Environment.IsDevelopment())
                builder.Services.AddScoped<IModerationService, MockModerationService>();
            else
                builder.Services.AddScoped<IModerationService, ClaudeModerationService>();

            var rootConfig = builder.Configuration
                          .AddJsonFile("appsettings.json")
                          .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                          .AddEnvironmentVariables()
                          .Build();

            rootConfig.GetConnectionString(YavscConstants.YavscConnectionStringName);   

            var app = await builder.ConfigureWebAppServices().ConfigurePipeline();
            app.Run();
        }
    }
}
