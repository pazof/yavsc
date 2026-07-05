
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Yavsc.Models;
using Yavsc.Models.Workflow;

namespace Yavsc.Helpers {
    public static class ListItemHelpers {

        public static List<SelectListItem> ActivityItems(
            this ApplicationDbContext _dbContext, List<UserActivity> activity)
        {
            var activities = activity.ToArray();
            var activityCodes = activities.Select(a=>a.DoesCode).ToArray();

            var systemActivities = _dbContext.Activities.Where(a=>!a.Moderated
            && activityCodes.Contains(a.Code)).ToArray();

            List<SelectListItem> items = systemActivities.Select(
            x=> new SelectListItem() {
                Value = x.Code, Text = x.Name, Selected = activities.Any(a=>a.DoesCode == x.Code)
                } ).ToList();

            return items;
        }
    }
}
