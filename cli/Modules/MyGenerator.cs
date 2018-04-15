

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace cli.Modules
{
    public class MyGenerator : IModule
    {

        ILogger logger;
        public MyGenerator(ILoggerFactory loggerfactory)
        {
            logger = loggerfactory.CreateLogger<MyGenerator>();
        }

        public void Run(string[] args)
        {
            logger.LogInformation(nameof(MyGenerator));
        }
    }
}