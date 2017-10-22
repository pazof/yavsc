using Microsoft.AspNet.Mvc;

namespace Yavsc.Controllers
{
    public class TestController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}