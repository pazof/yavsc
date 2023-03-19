using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;

namespace Yavsc.Server
{
    public class CliServerFactory : IServerFactory
    {
        public IFeatureCollection Initialize(IConfiguration configuration)
        {
            FeatureCollection featureCollection = new FeatureCollection();
            return featureCollection;
        }

        public IDisposable Start(IFeatureCollection serverFeatures, Func<IFeatureCollection, Task> application)
        {
            var task = application(serverFeatures);
            return task;
        }
    }
}
