
using System;
using System.IO;
using System.Web;
using Microsoft.AspNet.DataProtection.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Yavsc
{
    public partial class Startup
    {
        public void ConfigureProtectionServices(IServiceCollection services)
        {

            services.AddDataProtection();
            services.Add(ServiceDescriptor.Singleton(typeof(IApplicationDiscriminator),
                typeof(SystemWebApplicationDiscriminator)));

            services.ConfigureDataProtection(configure =>
            {
                configure.SetApplicationName(Configuration["Site:Title"]);
                configure.SetDefaultKeyLifetime(TimeSpan.FromDays(45));
                configure.PersistKeysToFileSystem(
                     new DirectoryInfo(Configuration["DataProtection:Keys:Dir"]));
            });
        }
        private sealed class SystemWebApplicationDiscriminator : IApplicationDiscriminator
        {
            private readonly Lazy<string> _lazyDiscriminator = new Lazy<string>(GetAppDiscriminatorCore);

            public string Discriminator => _lazyDiscriminator.Value;

            private static string GetAppDiscriminatorCore()
            {
                return HttpRuntime.AppDomainAppId;
            }
        }
    }
}
