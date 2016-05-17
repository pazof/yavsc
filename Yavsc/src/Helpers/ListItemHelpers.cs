
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.Rendering;
using Yavsc.Models;

namespace Yavsc {
    public static class ListItemHelpers {

        public static List<SelectListItem> ActivityItems(
            this ApplicationDbContext _dbContext, string selectedCode)
        {
            var codeIsNull = (string.IsNullOrEmpty(selectedCode));
            List<SelectListItem> items;
            if (codeIsNull) items = _dbContext.Activities.Select(
            x=> new SelectListItem() {
                Value=x.Code, Text=x.Name
                } ).ToList();
            else items =
             _dbContext.Activities.Select(
            x=> new SelectListItem() {
                Value=x.Code, Text=x.Name,
                Selected = (x.Code == selectedCode)
                } ).ToList();
            return items;
        }
    }
}