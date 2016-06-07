using Microsoft.AspNet.Mvc.Localization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Hosting;

namespace Yavsc.Controllers
{
    [ServiceFilter(typeof(LanguageActionFilter)),AllowAnonymous]
    public class HomeController : Controller
    {
        public IHostingEnvironment Hosting { get; set; }

        private readonly IHtmlLocalizer _localizer;

        public HomeController(IHtmlLocalizer<Startup> localizer, IHostingEnvironment hosting)
        {
            _localizer = localizer;
            Hosting = hosting;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {

            return View();
        }

        public ActionResult Chat()
        {
            return View();
        }

        public IActionResult Error()
        {
            var feature = this.HttpContext.Features.Get<IExceptionHandlerFeature>();

            return View("~/Views/Shared/Error.cshtml", feature?.Error);
        }
        public IActionResult Status(int id)
        {
            ViewBag.StatusCode = id;
             return View("~/Views/Shared/Status.cshtml");
        }
         public IActionResult Todo()
         {
             return View();
         }

    }
}
