using Microsoft.AspNet.Mvc;

namespace Yavsc.Controllers
{
    public class TestController: Controller
    {
        public IActionResult CalendarEventDateComponent()
        {
            return View();
        }
        public IActionResult MarkdownForms()
        {
            return View();
        }
    }
}