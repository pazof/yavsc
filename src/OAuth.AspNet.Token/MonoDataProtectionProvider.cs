


using System;
using System.IO;
using Microsoft.AspNet.DataProtection;

public class MonoDataProtectionProvider : IDataProtectionProvider
{
    private readonly string appName;

    public MonoDataProtectionProvider()
      : this(Guid.NewGuid().ToString())
    { }

    public MonoDataProtectionProvider(DirectoryInfo dataProtectionDirInfo)
    : this(Guid.NewGuid().ToString())
    {

    }
    public MonoDataProtectionProvider(string appName)
    {
        if (appName == null) { throw new ArgumentNullException("appName"); }
        this.appName = appName;
    }

    public IDataProtector Create(params string[] purposes)
    {
        if (purposes == null) { throw new ArgumentNullException("profile"); }

        return new MonoDataProtector(appName, purposes);
    }

    public IDataProtector CreateProtector(string purpose)
    {
        return Create(new string[] { purpose });
    }
}
