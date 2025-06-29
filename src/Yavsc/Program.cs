
using Microsoft.AspNetCore;
using Yavsc.Extensions;

namespace Yavsc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
       
            builder.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            var app = await builder.ConfigureWebAppServices().ConfigurePipeline();
            app.UseSession();
            app.Run();
        }
    }
}
