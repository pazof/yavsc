using Microsoft.AspNetCore.Mvc;
using Yavsc.Models;
using Yavsc.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace Yavsc.Controllers
{

    [AllowAnonymous]
    public class HomeController : Controller
    {
        readonly ApplicationDbContext _dbContext;

        readonly IHtmlLocalizer _localizer;

        private SiteSettings siteSettings;
        public HomeController(ILogger<HomeController> logger, 
        IHtmlLocalizer<Startup> localizer, 
        ApplicationDbContext context,
        IOptions<SiteSettings> settingsOptions)
        {
            _localizer = localizer;
            _dbContext = context;
            siteSettings = settingsOptions.Value;
        }

        public async Task<IActionResult> Index(string id)
        {
            ViewBag.IsFromSecureProx = Request.Headers.ContainsKey(Constants.SshHeaderKey) && Request.Headers[Constants.SshHeaderKey] == "on";
            ViewBag.SecureHomeUrl = "https://" + Request.Headers["X-Forwarded-Host"];
            ViewBag.SshHeaderKey = Request.Headers[Constants.SshHeaderKey];
            var uid = User.GetUserId();
            long[] clicked = null;
            if (uid == null)
            {
                // await HttpContext.Session.LoadAsync();
                var strclicked = HttpContext.Session.GetString("clicked");
                if (strclicked != null) clicked = strclicked.Split(':').Select(c => long.Parse(c)).ToArray();
                if (clicked == null) clicked = new long[0];
            }
            else clicked = _dbContext.DismissClicked.Where(d => d.UserId == uid).Select(d => d.NotificationId).ToArray();
            var notes = _dbContext.Notification.Where(
                n => !clicked.Contains(n.Id)
            );
            if (notes.Any()) this.Notify(notes);

            var toShow = _dbContext.Activities
               .Include(a => a.Forms)
               .Include(a => a.Parent)
               .Include(a => a.Children)
               .Where(a => !a.Hidden)
               .Where(a => a.ParentCode == id)
               .OrderByDescending(a => a.Rate).ToList();

            foreach (var a in toShow)
            {
                a.Children = a.Children.Where(c => !c.Hidden).ToList();
            }
            return View(toShow);
        }
        public async Task<IActionResult> About()
        {
            return View("About");
        }
        public IActionResult Privacy()
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
        public IActionResult Dash()
        {
            return View();
        }
        public ActionResult Chat()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.IsAuthenticated = true;
                string uid = User.GetUserId();
                ViewBag.Contacts = _dbContext.Contact.Where(c => c.OwnerId == uid)
                ;
            }
            else ViewBag.IsAuthenticated = false;
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
            User.GetUserId();

            return View();
        }

        public IActionResult VideoChat()
        {
            return View();
        }

        public IActionResult Audio()
        {
            return View();
        }

    }
}
