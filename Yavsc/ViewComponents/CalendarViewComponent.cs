using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Yavsc.Models;
using Yavsc.Models.Calendar;
using Yavsc.ViewModels.Calendar;

namespace Yavsc.ViewComponents
{
    public class CalendarViewComponent : ViewComponent
    {
        ApplicationDbContext _dbContext;
        ICalendarManager _manager;

        public CalendarViewComponent (
            ApplicationDbContext dbContext,
            ICalendarManager manager)
        {
            _manager = manager;
            _dbContext = dbContext;
        }
/* ,
Google.Apis Google.Apis.Core
        "Google.Apis.Auth": "1.27.1",
        "Google.Apis.Calendar.v3": "1.27.1.878"
        
         */


        public async Task<IViewComponentResult> InvokeAsync (
            string templateName,
            string htmlFieldName,
            string calId)
        {
            var minDate = DateTime.Now;
            var maxDate = minDate.AddDays(20);

            var cal = await _manager.GetCalendarAsync(
                calId, minDate, maxDate
            );

            ViewData["Calendar"] = cal;

            return View(templateName, new DateTimeChooserViewModel {
                InputId = htmlFieldName,
                MinDate = minDate,
                MaxDate = maxDate
            });
        }
    }
}