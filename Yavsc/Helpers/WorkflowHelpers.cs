

namespace Yavsc.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Data.Entity;
    using Yavsc.Models;
    using Yavsc.Services;
    using Yavsc.ViewModels.FrontOffice;

    public static class WorkflowHelpers
    {
        public static List<PerformerProfileViewModel> ListPerformers(this ApplicationDbContext context, 
        IBillingService billing,
        string actCode)
        {
            var settings = billing.GetPerformersSettingsAsync(actCode).Result?.ToArray();
           
            var actors = context.Performers
           .Include(p=>p.Activity)
           .Include(p=>p.Performer)
           .Include(p=>p.Performer.Posts)
           .Include(p=>p.Performer.Devices)
           .Where(p => p.Active && p.Activity.Any(u=>u.DoesCode==actCode)).OrderBy( x => x.Rate )
           .ToArray();
           List<PerformerProfileViewModel> result = new List<PerformerProfileViewModel> ();

           foreach (var perfer in actors)
           {
                var view = new PerformerProfileViewModel(perfer, actCode, settings?.FirstOrDefault(s => s.UserId == perfer.PerformerId));
                result.Add(view);
           }
           return result;
        }

    }
}
