// // NotWorking.cs
// /*
// paul  21/06/2018 10:16 20182018 6 21
// */
using System;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNet.Builder.Internal;
using Yavsc;
using Microsoft.Extensions.Logging;

namespace test
{
    [Collection("Yavsc dropped intents")]
    public class NotWorking : BaseTestContext
    {

        public void StringLocalizer()
        {
            var services = new ServiceCollection();
            // IHostingEnvironment env =  null;
            // IOptions<SiteSettings> siteSettings;

            services.AddTransient<IRuntimeEnvironment>(
                svs => PlatformServices.Default.Runtime
            );
            beforeCompileContext = YavscMandatory.CreateYavscCompilationContext();
            var prjDir = beforeCompileContext.ProjectContext.ProjectDirectory;
            YavscMandatory.ConfigureServices(services, prjDir, out configuration, out provider);

            IApplicationBuilder app = new ApplicationBuilder(provider);
            var rtd = app.Build();
            IOptions<LocalizationOptions> localOptions = Activator.CreateInstance<IOptions<LocalizationOptions>>(); ;
            // TODO build applicationEnvironment
            ResourceManagerStringLocalizerFactory strFact = new ResourceManagerStringLocalizerFactory
                (applicationEnvironment, localOptions);
            IStringLocalizer stringLocalizer = strFact.Create(typeof(NotWorking));
        }

        public void NoDnxEnv()
        {
           
            IOptions<LocalizationOptions> localOptions = Activator.CreateInstance<IOptions<LocalizationOptions>>(); 
            ResourceManagerStringLocalizerFactory strFact = new ResourceManagerStringLocalizerFactory(applicationEnvironment, localOptions);
            IStringLocalizer stringLocalizer = strFact.Create(typeof(NotWorking));
        }
    }
}
