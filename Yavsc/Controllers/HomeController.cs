using Microsoft.AspNet.Mvc.Localization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Hosting;
using Yavsc.Models;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Security.Claims;
using Microsoft.Data.Entity;

namespace Yavsc.Controllers
{
    [ServiceFilter(typeof(LanguageActionFilter)),AllowAnonymous]
    public class HomeController : Controller
    {
        public IHostingEnvironment Hosting { get; set; }

        private ApplicationDbContext DbContext;

        private readonly IHtmlLocalizer _localizer;

        public HomeController(IHtmlLocalizer<Startup> localizer, IHostingEnvironment hosting,
        ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _localizer = localizer;
            Hosting = hosting;
            DbContext = context;
        }

        public IActionResult Index()
        {
            return View(DbContext.Activities.Include(a=>a.Forms).OrderByDescending(a=>a.Rate));
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult AboutMarkdown()
        {
            return View();
        }

        public IActionResult Contact()
        {

            return View();
        }

        public ActionResult Chat()
        {
            if (User.Identity.IsAuthenticated) {
                ViewBag.IsAuthenticated=true;
                string uid = User.GetUserId();
                ViewBag.Contacts = DbContext.Contacts.Where(c=>c.OwnerId == uid)
                ;
            } else ViewBag.IsAuthenticated=false;
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
