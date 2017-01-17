using Yavsc.Models;
using YavscLib;

namespace Yavsc.Helpers
{
    public static class WorkflowHelpers
    {
        public static ISpecializationSettings  CreateSettings (this Activity activity) {
            if (activity.SettingsClassName==null) return null;
            var ctor = Startup.ProfileTypes[activity.SettingsClassName].GetConstructor(System.Type.EmptyTypes);
            if (ctor==null) return null;
            return (ISpecializationSettings) ctor.Invoke(null);
        }
    }
}