using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting.Server;
using Microsoft.AspNet.Http.Features;
using Microsoft.Extensions.Configuration;
using Yavsc.Models;

namespace Yavsc.Server
{
    public class cliServerFactory : IServerFactory
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
