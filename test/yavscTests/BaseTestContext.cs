using System;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit.Abstractions;

namespace yavscTests
{
    public class BaseTestContext {

        protected IApplicationEnvironment applicationEnvironment = null;
        protected IServiceProvider serviceProvider = null;
        protected IConfigurationRoot configurationRoot;
        protected BeforeCompileContext beforeCompileContext;
        protected IServiceProvider provider;
        protected IConfigurationRoot configuration;
        protected readonly ITestOutputHelper _output;
        protected ServerSideFixture _serverFixture;

        public BaseTestContext(ITestOutputHelper output, ServerSideFixture serverFixture)
        {
            this._output = output;
            this._serverFixture = serverFixture;
        }
    }
}
