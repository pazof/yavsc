

namespace Yavsc.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Data.Entity;
    using Models.Workflow;
    using Yavsc.Models;
    using YavscLib;
    public static class WorkflowHelpers
    {
        public static ISpecializationSettings  CreateSettings (this Activity activity) {
            if (activity.SettingsClassName==null) return null;
            var ctor = Startup.ProfileTypes[activity.SettingsClassName].GetConstructor(System.Type.EmptyTypes);
            if (ctor==null) return null;
            return (ISpecializationSettings) ctor.Invoke(null);
        }

        public static List<PerformerProfile> ListPerformers(this ApplicationDbContext context, string actCode)
        {
            return context.Performers
           .Include(p=>p.Activity)
           .Include(p=>p.Performer)
           .Include(p=>p.Performer.Posts)
           .Include(p=>p.Performer.Devices)
           .Where(p => p.Active && p.Activity.Any(u=>u.DoesCode==actCode)).OrderBy( x => x.Rate ).ToList(); 
        }
    }
}