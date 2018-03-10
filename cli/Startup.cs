using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Framework.Configuration;
using Newtonsoft.Json;
using Yavsc;
using Yavsc.Models;
using Yavsc.Auth;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace cli
{
    public  class Startup
    {

        public string HostingFullName { get; private set; }

        public static SiteSettings SiteSetup { get; private set; }
        public static SmtpSettings SmtpSettup { get; private set; }
        public IConfigurationRoot Configuration { get; set; }
        public string ConnectionString { get; private set; }
		public static MonoDataProtectionProvider ProtectionProvider { get; private set; }
		public static IdentityOptions AppIdentityOptions { get; private set; }

        // Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
    {
        System.Console.WriteLine("Configuring services ...");
        services.AddEntityFramework()
              .AddNpgsql()
              .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration["Data:DefaultConnection:ConnectionString"]))
              ;
        services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            services.AddOptions();
           
            services.Configure<SiteSettings>((o)=> JsonConvert.PopulateObject(Configuration["Site"],o));
            services.Configure<SmtpSettings>((o)=> JsonConvert.PopulateObject(Configuration["Smtp"],o));

			ProtectionProvider = new MonoDataProtectionProvider(Configuration["Site:Title"]); ;
			services.AddInstance<MonoDataProtectionProvider>
			(ProtectionProvider);


			
    }


        // Use this method to configure the HTTP request pipeline.
     public void Configure(IApplicationBuilder app, IOptions<SiteSettings> siteSettingsOptions, IOptions<SmtpSettings> smtpSettingsOptions)
    {
        System.Console.WriteLine("Configuring application ...");
        SiteSetup = siteSettingsOptions.Value;
        SmtpSettup = smtpSettingsOptions.Value;
    }

		public Startup(IHostingEnvironment env, IApplicationBuilder app)
        {
            var devtag = env.IsDevelopment()?"D":"";
            var prodtag = env.IsProduction()?"P":"";
            var stagetag = env.IsStaging()?"S":"";

            HostingFullName = $" [{env.EnvironmentName}:{prodtag}{devtag}{stagetag}]";
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);


            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                // builder.AddUserSecrets();
            }
            Configuration = builder.Build();
            ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];

        }
        
    }
}