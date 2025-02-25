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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Authentication;
using static OAuth.AspNet.AuthServer.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Razor;

namespace yavscTests
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
            _serverFixture.EnsureWeb();
        }

    }
}
