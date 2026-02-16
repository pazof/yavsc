using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Models;
using Xunit.Abstractions;
using Yavsc.Server.Models.IT.SourceCode;
using Microsoft.EntityFrameworkCore;
using isnd.tests;

namespace yavscTests
{
    [Collection("Yavsc Server")]
    [Trait("regression", "oui")]
    public class BaseTestContext:  IClassFixture<WebServerFixture>, IDisposable
    {
        public readonly WebServerFixture _serverFixture;
        private readonly ITestOutputHelper _output;

        public BaseTestContext(ITestOutputHelper output, WebServerFixture fixture)
        {
            this._serverFixture = fixture;
            this._output = output;
        }

        [Fact]
        public void GitClone()
        {
            using var scope = _serverFixture.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.NotNull (dbContext.Project);
            var firstProject = dbContext.Project.Include(p=>p.Repository).FirstOrDefault();
            Assert.NotNull (firstProject);
            var di = new DirectoryInfo(_serverFixture.SiteSettings.GitRepository);
            if (!di.Exists) di.Create();

            var clone = new GitClone(_serverFixture.SiteSettings.GitRepository);
            clone.Launch(firstProject);
            gitRepo = di.FullName;
        }
        string gitRepo=null;
        private IConfigurationRoot configurationRoot;


        [Fact]
        public void HaveConfigurationRoot()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile( "appsettings.json", false);
            builder.AddJsonFile( "appsettings.Development.json", true);
            configurationRoot = builder.Build();
        }

        public void Dispose()
        {
            if (gitRepo!=null)
            {
                Directory.Delete(Path.Combine(gitRepo,"yavsc"), true);
            }
        }
    }
}
