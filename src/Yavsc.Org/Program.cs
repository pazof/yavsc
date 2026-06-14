using Anthropic.SDK;
using Yavsc.Abstract.Interfaces;
using Yavsc.Extensions;
using Yavsc.Server.Helpers;

namespace Yavsc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddConfiguration("org");
            var app = await builder
                .ConfigureWebAppServices()
                .ConfigurePipeline();

            app.Run();
        }

        
    }
}