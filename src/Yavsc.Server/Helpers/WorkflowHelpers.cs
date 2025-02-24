

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
        public static async Task<List<PerformerProfileViewModel>> ListPerformersAsync(this ApplicationDbContext context, 
        IBillingService billing,
        string actCode)
        {
           
            var actors = context.Performers
           .Include(p=>p.Activity)
           .Include(p=>p.Performer)
           .Where(p => p.Active && p.Activity.Any(u=>u.DoesCode==actCode)).OrderBy( x => x.Rate )
           .ToArray();

            List<PerformerProfileViewModel> result = new ();
            foreach (var a in actors)
            {
                var settings = await billing.GetPerformersSettingsAsync(actCode, a.PerformerId);
                result.Add(new PerformerProfileViewModel(a, actCode,settings));
            }

            return result;
        }

    }
}
