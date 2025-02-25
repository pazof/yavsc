
using Microsoft.AspNetCore;
using Yavsc.Extensions;

namespace Yavsc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
       
            builder.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            var app = builder.ConfigureWebAppServices().ConfigurePipeline();
            app.UseSession();
            app.Run();
        }
    }
}
