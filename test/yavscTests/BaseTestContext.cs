
using Microsoft.Extensions.Configuration;

namespace yavscTests
{
    public class BaseTestContext {

        protected IServiceProvider serviceProvider = null;
        protected IConfigurationRoot configurationRoot;
        protected IServiceProvider provider;
        protected IConfigurationRoot configuration;
        protected ServerSideFixture _serverFixture;

        public BaseTestContext( ServerSideFixture serverFixture)
        {
            this._serverFixture = serverFixture;
        }
    }
}
