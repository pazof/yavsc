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
    using System.IO;
    using Models;
    using Yavsc;
    using Yavsc.Helpers;

    [AllowAnonymous]
    public class HomeController : Controller
    {
        IHostingEnvironment _hosting;

        ApplicationDbContext _dbContext;

        readonly IHtmlLocalizer _localizer;
        public HomeController(IHtmlLocalizer<Startup> localizer, IHostingEnvironment hosting,
        ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _localizer = localizer;
            _hosting = hosting;
            _dbContext = context;
        }

        public async Task<IActionResult> Index(string id)
        {
            ViewBag.IsFromSecureProx = (Request.Headers.ContainsKey(Constants.SshHeaderKey)) ? Request.Headers[Constants.SshHeaderKey] == "on" : false;
            ViewBag.SecureHomeUrl = "https://" + Request.Headers["X-Forwarded-Host"];
            ViewBag.SshHeaderKey = Request.Headers[Constants.SshHeaderKey];
            var uid = User.GetUserId();
            long[] clicked = null;
            if (uid == null)
            {
                await HttpContext.Session.LoadAsync();
                var strclicked = HttpContext.Session.GetString("clicked");
                if (strclicked != null) clicked = strclicked.Split(':').Select(c => long.Parse(c)).ToArray();
                if (clicked == null) clicked = new long[0];
            }
            else clicked = _dbContext.DimissClicked.Where(d => d.UserId == uid).Select(d => d.NotificationId).ToArray();
            var notes = _dbContext.Notification.Where(
                n => !clicked.Contains(n.Id)
            );
            this.Notify(notes);

            ViewData["HaircutCommandCount"] = _dbContext.HairCutQueries.Where(
                q => q.ClientId == uid && q.Status < QueryStatus.Failed
            ).Count();
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
            FileInfo fi = new FileInfo("wwwroot/version");
            return View("About", fi.Exists ? _localizer["Version logicielle: "] + await fi.OpenText().ReadToEndAsync() : _localizer["Aucune information sur la version logicielle n'est publiée."]);
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
