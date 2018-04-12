using System;
using System.Runtime.Versioning;
using Microsoft.Extensions.PlatformAbstractions;

namespace cli_2
{
    public class ApplicationEnvironment : IApplicationEnvironment
{
    internal ApplicationEnvironment( FrameworkName targetFramework, string appRootDir )
    {
        this.RuntimeFramework = targetFramework;
        ApplicationBasePath = appRootDir;
    }
    public string ApplicationBasePath
    {
        get; set;
    }

    public string ApplicationName
    {
        get; set;
    }

    public string ApplicationVersion
    {
        get; set;
    }

    public string Configuration
    {
        get; set;
    }

    public FrameworkName RuntimeFramework
    {
        get; set;
    }

        public string Version
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object GetData(string name)
        {
            throw new NotImplementedException();
        }

        public void SetData(string name, object value)
        {
            throw new NotImplementedException();
        }
    }
}