

namespace Yavsc.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
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
           .Include(p=>p.Performer.DeviceDeclaration)
           .Where(p => p.Active && p.Activity.Any(u=>u.DoesCode==actCode)).OrderBy( x => x.Rate )
           .ToArray();
            List<PerformerProfileViewModel> result = new List<PerformerProfileViewModel> ();
            result.AddRange(
                    actors.Select(a=> new PerformerProfileViewModel(a, actCode, settings?.FirstOrDefault(s =>   s.UserId == a.PerformerId))));

            return result;
        }

    }
}
