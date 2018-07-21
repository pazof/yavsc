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
using Xunit.Abstractions;
using Yavsc;
using Yavsc.Lib;
using Yavsc.Models;

namespace test
{
    [Collection("Yavsc Work In Progress")]
    public class YavscWorkInProgress : BaseTestContext
    {

        ServerSideFixture _serverFixture;
        ITestOutputHelper output;
        public YavscWorkInProgress(ServerSideFixture serverFixture, ITestOutputHelper output)
        {
            this.output = output;
            _serverFixture = serverFixture;
        }
        
        [Fact]
        public void GitClone()
        {
            
          var dbc =  _serverFixture._app.Services.GetService(typeof(ApplicationDbContext)) as  ApplicationDbContext;

          
            var firstProject = dbc.Projects.FirstOrDefault();
            Assert.NotNull (firstProject);

            var clone = new GitClone(_serverFixture._siteSetup.GitRepository);
            clone.Launch(firstProject);
        }
    }
}
