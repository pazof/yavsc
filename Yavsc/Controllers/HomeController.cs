using Microsoft.AspNet.Mvc.Localization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Security.Claims;
using Microsoft.Data.Entity;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace Yavsc.Controllers
{
    using Models;

    [AllowAnonymous]
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

        public async Task<IActionResult> Index(string id)
        {
            ViewBag.IsFromSecureProx = (Request.Headers.ContainsKey(Constants.SshHeaderKey))?  Request.Headers[Constants.SshHeaderKey]=="on" : false ;
            ViewBag.SecureHomeUrl = "https://"+Request.Headers["X-Forwarded-Host"];
            ViewBag.SshHeaderKey = Request.Headers[Constants.SshHeaderKey];
            var uid = User.GetUserId();
            long [] clicked=null;
            if (uid==null) {
                await HttpContext.Session.LoadAsync();
                var strclicked = HttpContext.Session.GetString("clicked");
                if (strclicked!=null) clicked = strclicked.Split(':').Select(c=>long.Parse(c)).ToArray();
              if (clicked==null) clicked = new long [0];
            }
            else clicked = DbContext.DimissClicked.Where(d=>d.UserId == uid).Select(d=>d.NotificationId).ToArray();
            var notes = DbContext.Notification.Where(
                n=> !clicked.Any(c=>n.Id==c)
            );
            ViewData["Notify"] = notes;
            return View(DbContext.Activities.Where(a=>a.ParentCode==id && !a.Hidden).Include(a=>a.Forms).Include(a=>a.Children)
            .OrderByDescending(a=>a.Rate));
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
