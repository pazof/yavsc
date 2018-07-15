// // YavscWorkInProgress.cs
// /*
// paul  21/06/2018 10:11 20182018 6 21
// */
using System;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Builder.Internal;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Dnx.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using Yavsc;
using Yavsc.Lib;
using Yavsc.Models;

namespace Test
{
    [Collection("Yavsc Work In Progress")]
    public class YavscWorkInProgress : BaseTestContext
    {

        public void GitClone()
        {
            
            AppDomain.CurrentDomain.SetData("YAVSC_DB_CONNECTION", "Server=localhost;Port=5432;Database=YavscDev;Username=yavscdev;Password=admin;");
            ServiceCollection services = new ServiceCollection();

            YavscMandatory.ConfigureServices(services, testprojectAssetPath, out configuration, out provider);

            var siteConfig = provider.GetRequiredService<IOptions<SiteSettings>>().Value;
            var dbc = provider.GetRequiredService<ApplicationDbContext>();
          
            var firstProject = dbc.Projects.FirstOrDefault();
            Assert.NotNull (firstProject);

            var clone = new GitClone(siteConfig.GitRepository);
            clone.Launch(firstProject);
        }
    }
}
