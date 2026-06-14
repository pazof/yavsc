// See https://aka.ms/new-console-template for more information

using cli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
internal class Program
{
    public static IHost AppHost { get; private set; }
    public static IConfigurationRoot AppConfiguration { get; private set; }
    public static IHostEnvironment AppEnvironment { get; private set; }

    private static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
         builder.Configuration
                .AddJsonFile("appsettings-cli.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings-cli.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
            
        AppHost = builder.Build();
        AppConfiguration = builder.Configuration;
        AppHost.Start();
        AppEnvironment = builder.Environment;
        Console.WriteLine("Hello, World!");
    }
}
