using System.Resources;
using Yavsc.Resources;

public static class ResourcesHelpers {

    static ResourceManager _defaultResourceManager 
    = ResourceManager.CreateFileBasedResourceManager("Yavsc.Localization",".",typeof(YavscLocalisation));

    public static ResourceManager DefaultResourceManager
    { 
        get { return _defaultResourceManager; }
        set { _defaultResourceManager = value; }
    }
}