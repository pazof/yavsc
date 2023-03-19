
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
            List<SelectListItem> items = _dbContext.Activities.Select(
            x=> new SelectListItem() {
                Value = x.Code, Text = x.Name, Selected = activity.Any(a=>a.DoesCode == x.Code)
                } ).ToList();
           
            return items;
        }
    }
}
