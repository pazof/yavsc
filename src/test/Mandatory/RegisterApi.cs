// // YavscWorkInProgress.cs
// /*
// paul  21/06/2018 10:11 20182018 6 21
// */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Authentication;
using static OAuth.AspNet.AuthServer.Constants;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.Razor;

namespace test
{
    [Collection("Yavsc Work In Progress")]
    [Trait("regression", "oui")]
    [Trait("module", "api")]
    public class RegiserAPI : BaseTestContext, IClassFixture<ServerSideFixture>
    {
        public RegiserAPI(ServerSideFixture serverFixture, ITestOutputHelper output)
        : base(output, serverFixture)
        {
            
        }

        [Fact]
        public void EnsureWeb()
        {
            var host = new WebHostBuilder();
             host.UseEnvironment("Development")
            .UseServer("Microsoft.AspNet.Server.Kestrel")
            .UseStartup<test.Startup>()
            .Build().Start();
        }

    }
}
