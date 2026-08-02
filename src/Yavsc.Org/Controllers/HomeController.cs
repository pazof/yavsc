using Microsoft.AspNetCore.Mvc;
using Yavsc.Models;
using Yavsc.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Yavsc.Server.Helpers;

namespace Yavsc.Controllers
{

    [AllowAnonymous]
    public class HomeController : Controller
    {
        readonly ApplicationDbContext _dbContext;
        readonly ILogger<HomeController> _logger;
        private readonly bool _isDevelopment;
        readonly IHtmlLocalizer _localizer;

        private SiteSettings siteSettings;
        public HomeController(ILogger<HomeController> logger, 
        IHtmlLocalizer<HomeController> localizer, 
        ApplicationDbContext context,
        IOptions<SiteSettings> settingsOptions, 
        IWebHostEnvironment env
        )
        {
            _localizer = localizer;
            _dbContext = context;
            siteSettings = settingsOptions.Value;
            _logger = logger;
            _isDevelopment = env.IsDevelopment();

        }

        public async Task<IActionResult> Index(string id)
        {
            ViewBag.IsFromSecureProx = Request.Headers.ContainsKey(YavscConstants.SshHeaderKey) && Request.Headers[YavscConstants.SshHeaderKey] == "on";
            ViewBag.SecureHomeUrl = "https://" + Request.Headers["X-Forwarded-Host"];
            ViewBag.SshHeaderKey = Request.Headers[YavscConstants.SshHeaderKey];
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
            return View(siteSettings);
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
            if (_isDevelopment)
            {
                _logger.LogInformation(
                    "Home/Error requested in Development. This endpoint is disabled because DeveloperExceptionPage should handle unhandled exceptions.");

                return NotFound(
                    "In Development, /Home/Error is disabled. Unhandled exceptions are rendered by DeveloperExceptionPage.");
            }

            var errorViewModel = new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier
            };

            var exceptionHandlerPathFeature =
            HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature is null)
            {
                _logger.LogWarning(
                    "Home/Error called without IExceptionHandlerPathFeature in non-development environment.");

                return View("~/Views/Shared/Error.cshtml", errorViewModel);
            }

            if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
            {
                errorViewModel.Description = "The file was not found.";
            }

            if (exceptionHandlerPathFeature?.Path == "/")
            {
                errorViewModel.Description ??= string.Empty;
                errorViewModel.Description += " Page: Home.";
            }
    
          
            return View("~/Views/Shared/Error.cshtml", errorViewModel);
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
