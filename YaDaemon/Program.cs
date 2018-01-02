using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.ConfigurationModel;
using Yavsc;


public class Program : ServiceBase
{
    private readonly EventLog _log =
           new EventLog("Application") { Source = "Application" };
    private readonly IServiceProvider _serviceProvider;
    
    private IHostingEngine _hostingEngine;
    private IDisposable _shutdownServerDisposable;
    private static Program instance;
    public Program(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        instance = this;
    }
    // Fails claming "use mono-service" : 
    // public static void Main(string[] args) => YaDaemon.YaDaemon.Main(args);
    public static void Main(string[] args) => YaDaemon.YaDaemon.Main(args); 

    // public static void Main(string[] args) =>  Console.WriteLine("Hello World!");

    public void OldMain(string[] args)
    {
        Microsoft.Extensions.PlatformAbstractions.IApplicationEnvironment iappenv;
        IHostingEnvironment env = new Microsoft.AspNet.Hosting.HostingEnvironment();
        iappenv = null;// new HostingEnvironmentExtensions();
        Console.WriteLine("HW");
         
        _log.WriteEntry("Test from MyDnxService.", EventLogEntryType.Information, 1);
        
        

        #if DEBUG

          OnStart(null);
        #else 
        Run(this);
        #endif
    }

    protected override void OnStart(string[] args)
    {
        /* TODO how to use configSource
        var configSource = new MemoryConfigurationSource();
        configSource.Add("server.urls", "http://localhost:5000");
        configSource.Load();
        */
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection();
        configBuilder.SetBasePath("../Yavsc/");
        configBuilder.AddJsonFile("../Yavsc/project.json");
        var config = configBuilder.Build();
         
        var builder = new WebHostBuilder();

        builder.UseServer("Microsoft.AspNet.Server.Kestrel");
        builder.UseServices(services => services.AddMvc());
        builder.UseStartup(appBuilder =>
        {
            appBuilder.UseDefaultFiles();
            appBuilder.UseStaticFiles();
            appBuilder.UseMvc();
        });

        _hostingEngine = builder.Build();
       // Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);

        _shutdownServerDisposable = _hostingEngine.Start();
        
    }
}



