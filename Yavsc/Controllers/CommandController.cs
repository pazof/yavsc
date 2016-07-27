using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Booking;
using Yavsc.Models.Google.Messaging;
using Yavsc.Services;

namespace Yavsc.Controllers
{
    [ServiceFilter(typeof(LanguageActionFilter))]
    public class CommandController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private ApplicationDbContext _context;
        private GoogleAuthSettings _googleSettings;
        private IGoogleCloudMessageSender _GCMSender;
        private IEmailSender _emailSender;
        private IStringLocalizer _localizer;
        SiteSettings _siteSettings;
        SmtpSettings _smtpSettings;

        private readonly ILogger _logger;
        public CommandController(ApplicationDbContext context, IOptions<GoogleAuthSettings> googleSettings,
        IGoogleCloudMessageSender GCMSender,
          UserManager<ApplicationUser> userManager,
          IStringLocalizer<Yavsc.Resources.YavscLocalisation> localizer,
          IEmailSender emailSender,
          IOptions<SmtpSettings> smtpSettings,
          IOptions<SiteSettings> siteSettings,
          ILoggerFactory loggerFactory)
        {
            _context = context;
            _GCMSender = GCMSender;
            _emailSender = emailSender;
            _googleSettings = googleSettings.Value;
            _userManager = userManager;
            _smtpSettings = smtpSettings.Value;
            _siteSettings = siteSettings.Value;
            _localizer = localizer;
            _logger = loggerFactory.CreateLogger<CommandController>();
        }

        // GET: Command
        public IActionResult Index()
        {
            return View(_context.BookQueries
            .Include(x => x.Client)
            .Include(x => x.PerformerProfile)
            .Include(x => x.PerformerProfile.Performer)
            .Include(x => x.Location)
            .Include(x => x.Bill).ToList());
        }

        // GET: Command/Details/5
        public IActionResult Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            BookQuery command = _context.BookQueries
            .Include(x => x.Location)
            .Include(x => x.PerformerProfile)
            .Single(m => m.Id == id);
            if (command == null)
            {
                return HttpNotFound();
            }

            return View(command);
        }

        [Authorize]
        /// <summary>
        /// Gives a view on
        /// Creating a command for a specified performer
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Create(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException(
                    "This method needs a performer id"
                );
            var pro = _context.Performers.Include(
                x => x.Performer).FirstOrDefault(
                x => x.PerformerId == id
            );
            if (pro == null)
                return HttpNotFound();

            ViewBag.GoogleSettings = _googleSettings;
            var userid = User.GetUserId();
            var user = _userManager.FindByIdAsync(userid).Result;
            return View(new BookQuery(new Location(),DateTime.Now.AddHours(4))
            {
                PerformerProfile = pro,
                PerformerId = pro.PerformerId,
                ClientId = userid,
                Client = user
            });
        }

        // POST: Command/Create
        [HttpPost, Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookQuery command)
        {
            var uid = User.GetUserId();
            var prid = command.PerformerId;
            if (string.IsNullOrWhiteSpace(uid)
            || string.IsNullOrWhiteSpace(prid))
                throw new InvalidOperationException(
                    "This method needs a prid and uid"
                );
            var pro = _context.Performers.Include(
                u => u.Performer
            ).Include( u => u.Performer.Devices)
            .FirstOrDefault(
                x => x.PerformerId == command.PerformerId
            );
            command.PerformerProfile = pro;
            var user = await _userManager.FindByIdAsync(
               User.GetUserId()
           );
            command.Client = user;
            if (ModelState.IsValid)
            {
                var yaev = command.CreateEvent(_localizer);
                MessageWithPayloadResponse grep = null;

                _context.Attach<Location>(command.Location);
                _context.BookQueries.Add(command, GraphBehavior.IncludeDependents);
                _context.SaveChanges();

                if (pro.AcceptNotifications
                && pro.AcceptPublicContact)
                {
                    if (pro.Performer.Devices.Count > 0) {
                        var regids = command.PerformerProfile.Performer
                        .Devices.Select(d => d.GCMRegistrationId);
                        var sregids = string.Join(",",regids);
                        _logger.LogWarning($"ApiKey: {_googleSettings.ApiKey}"); 
                        _logger.LogWarning($"RegIds: {sregids}");
                        grep = await _GCMSender.NotifyAsync(_googleSettings,regids,yaev);
                    }
                    // TODO setup a profile choice to allow notifications
                    // both on mailbox and mobile
                    // if (grep==null || grep.success<=0 || grep.failure>0)
                    ViewBag.GooglePayload=grep;
                    if (grep!=null)
                      _logger.LogWarning($"Performer: {command.PerformerProfile.Performer.UserName} success: {grep.success} failure: {grep.failure}");

                    await _emailSender.SendEmailAsync(
                        _siteSettings, _smtpSettings,
                        command.PerformerProfile.Performer.Email,
                        yaev.Title,
                        $"{yaev.Description}\r\n-- \r\n{yaev.Comment}\r\n"
                    );
                }
                ViewBag.GoogleSettings = _googleSettings;
                return View("CommandConfirmation",command);
            }
            ViewBag.GoogleSettings = _googleSettings;
            return View(command);
        }

        // GET: Command/Edit/5
        public IActionResult Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            BookQuery command = _context.BookQueries.Single(m => m.Id == id);
            if (command == null)
            {
                return HttpNotFound();
            }
            return View(command);
        }

        // POST: Command/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookQuery command)
        {
            if (ModelState.IsValid)
            {
                _context.Update(command);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(command);
        }

        // GET: Command/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            BookQuery command = _context.BookQueries.Single(m => m.Id == id);
            if (command == null)
            {
                return HttpNotFound();
            }

            return View(command);
        }

        // POST: Command/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            BookQuery command = _context.BookQueries.Single(m => m.Id == id);
            _context.BookQueries.Remove(command);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
