using Anthropic.SDK;
using Yavsc.Abstract.Interfaces;
using Yavsc.Extensions;

namespace Yavsc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration
                .AddJsonFile("appsettings-org.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings-org.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables();
            var app = await builder
                .ConfigureWebAppServices()
                .ConfigurePipeline();

            app.Run();
        }
    }
}