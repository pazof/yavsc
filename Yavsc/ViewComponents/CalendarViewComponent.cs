using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Yavsc.Models;
using Yavsc.Services;

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

        public async Task<IViewComponentResult> InvokeAsync (
            string htmlFieldName,
            string calId = null)
        {
            var minDate = DateTime.Now;
            var maxDate = minDate.AddDays(20);

            var model = await _manager.CreateViewModelAsync(
                htmlFieldName,
                calId, minDate, maxDate
            );

            return View(model);
        }
        public async Task<IViewComponentResult> InvokeAsync (
            string htmlFieldName)
        {
            var minDate = DateTime.Now;
            var maxDate = minDate.AddDays(20);

            var model = await _manager.CreateViewModelAsync(
                htmlFieldName,
                null, minDate, maxDate
            );

            return View(model);
        }
    }
}