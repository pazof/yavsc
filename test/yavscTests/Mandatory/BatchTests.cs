using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Xunit;
using Yavsc;
using Yavsc.Models;
using Yavsc.Services;
using System.Runtime.Versioning;
using System.Diagnostics;
using Yavsc.Helpers;
using Xunit.Abstractions;
using Yavsc.Server.Models.IT.SourceCode;
using yavscTests.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.AspNetCore.Identity;
using Yavsc.Settings;
using Microsoft.AspNetCore.Razor.Language;
using isnd.tests;

namespace yavscTests
{
    [Collection("Yavsc mandatory success story")]
    [Trait("regression", "oui")]
    public class BaseTestContext:  IClassFixture<WebServerFixture>, IDisposable
    {
        public readonly WebServerFixture _serverFixture;
        private readonly TestingSetup _testingOptions;
        private readonly ITestOutputHelper _output;

        public BaseTestContext(ITestOutputHelper output, WebServerFixture fixture)
        {
            this._serverFixture = fixture;
            _testingOptions = fixture.TestingSetup;
            this._output = output;
        }

        [Fact]
        public void GitClone()
        {
            Assert.NotNull (_serverFixture.DbContext.Project);
            var firstProject = _serverFixture.DbContext.Project.Include(p=>p.Repository).FirstOrDefault();
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
