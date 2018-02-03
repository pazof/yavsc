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

namespace Yavsc.Controllers
{
    using Helpers;
    using Models;
    using Models.Google.Messaging;
    using Models.Relationship;
    using Models.Workflow;
    using Services;

    public class CommandController : Controller
    {
        protected UserManager<ApplicationUser> _userManager;
        protected ApplicationDbContext _context;
        protected GoogleAuthSettings _googleSettings;
        protected IGoogleCloudMessageSender _GCMSender;
        protected IEmailSender _emailSender;
        protected IStringLocalizer _localizer;
        protected SiteSettings _siteSettings;
        protected SmtpSettings _smtpSettings;

        protected ICalendarManager _calendarManager;

        protected readonly ILogger _logger;
        public CommandController(ApplicationDbContext context, IOptions<GoogleAuthSettings> googleSettings,
        IGoogleCloudMessageSender GCMSender,
          UserManager<ApplicationUser> userManager,
          ICalendarManager calendarManager,
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
            _calendarManager = calendarManager;
            _localizer = localizer;
            _logger = loggerFactory.CreateLogger<CommandController>();
        }

        // GET: Command
        [Authorize]
        public virtual async Task<IActionResult> Index()
        {
            var uid = User.GetUserId();
            return View(await _context.RdvQueries
            .Include(x => x.Client)
            .Include(x => x.PerformerProfile)
            .Include(x => x.PerformerProfile.Performer)
            .Include(x => x.Location)
            .Where(x=> x.ClientId == uid || x.PerformerId == uid)
            .ToListAsync());
        }

        // GET: Command/Details/5
        public virtual async Task<IActionResult> RdvDetails(long id)
        {
            RdvQuery command = await _context.RdvQueries
            .Include(x => x.Location)
            .Include(x => x.PerformerProfile)
            .SingleAsync(m => m.Id == id);
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
        public IActionResult Create(string id, string activityCode)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException(
                    "This method needs a performer id"
                );
             if (string.IsNullOrWhiteSpace(activityCode))
                throw new InvalidOperationException(
                    "This method needs an activity code"
                );
            var pro = _context.Performers.Include(
                x => x.Performer).FirstOrDefault(
                x => x.PerformerId == id
            );
            if (pro == null)
                return HttpNotFound();
            ViewBag.Activity =  _context.Activities.FirstOrDefault(a=>a.Code == activityCode);
            ViewBag.GoogleSettings = _googleSettings;
            var userid = User.GetUserId();
            var user = _userManager.FindByIdAsync(userid).Result;
            return View(new RdvQuery(activityCode,new Location(),DateTime.Now.AddHours(4))
            {
                PerformerProfile = pro,
                PerformerId = pro.PerformerId,
                ClientId = userid,
                Client = user,
                ActivityCode = activityCode
            });
        }

        // POST: Command/Create
        [HttpPost, Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RdvQuery command)
        {

            var uid = User.GetUserId();
            var prid = command.PerformerId;
            if (string.IsNullOrWhiteSpace(uid)
            || string.IsNullOrWhiteSpace(prid))
                throw new InvalidOperationException(
                    "This method needs a PerformerId"
                );
            var pro = _context.Performers.Include(
                u => u.Performer
            ).Include(u => u.Performer.Devices)
            .FirstOrDefault(
                x => x.PerformerId == command.PerformerId
            );
            var user = await _userManager.FindByIdAsync(uid);
            command.Client = user;
            command.ClientId = uid;
            command.PerformerProfile = pro;
            // FIXME Why!!
            // ModelState.ClearValidationState("PerformerProfile.Avatar");
            // ModelState.ClearValidationState("Client.Avatar");
            // ModelState.ClearValidationState("ClientId");
            ModelState.MarkFieldSkipped("ClientId");

            if (ModelState.IsValid)
                {
                var existingLocation = _context.Locations.FirstOrDefault( x=>x.Address == command.Location.Address
                && x.Longitude == command.Location.Longitude && x.Latitude == command.Location.Latitude );

                if (existingLocation!=null) {
                    command.Location=existingLocation;
                }
                else _context.Attach<Location>(command.Location);
                _context.RdvQueries.Add(command, GraphBehavior.IncludeDependents);
                _context.SaveChanges(User.GetUserId());

                var yaev = command.CreateEvent(_localizer, "NewCommand");

                MessageWithPayloadResponse grep = null;

                if (pro.AcceptNotifications
                && pro.AcceptPublicContact)
                {
                    if (pro.Performer.Devices.Count > 0) {
                        var regids = command.PerformerProfile.Performer
                        .Devices.Select(d => d.GCMRegistrationId);
                        grep = await _GCMSender.NotifyBookQueryAsync(_googleSettings,regids,yaev);
                    }
                    
                    // TODO setup a profile choice to allow notifications
                    // both on mailbox and mobile
                    // if (grep==null || grep.success<=0 || grep.failure>0)
                    ViewBag.GooglePayload=grep;

                    ViewBag.EmailSent = await _emailSender.SendEmailAsync(
                        _siteSettings, _smtpSettings,
                        command.PerformerProfile.Performer.UserName,
                        command.PerformerProfile.Performer.Email,
                         $"{command.Client.UserName} (un client) vous demande un rendez-vous",
                        $"{yaev.CreateBody()}\r\n-- \r\n{yaev.Previsional}\r\n{yaev.EventDate}\r\n"
                    );
                }
                ViewBag.Activity =  _context.Activities.FirstOrDefault(a=>a.Code == command.ActivityCode);
                ViewBag.GoogleSettings = _googleSettings;
                return View("CommandConfirmation",command);
            }
            ViewBag.Activity =  _context.Activities.FirstOrDefault(a=>a.Code == command.ActivityCode);
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

            RdvQuery command = _context.RdvQueries.Single(m => m.Id == id);
            if (command == null)
            {
                return HttpNotFound();
            }
            return View(command);
        }

        // POST: Command/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RdvQuery command)
        {
            if (ModelState.IsValid)
            {
                _context.Update(command);
                _context.SaveChanges(User.GetUserId());
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

            RdvQuery command = _context.RdvQueries.Single(m => m.Id == id);
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
            RdvQuery command = _context.RdvQueries.Single(m => m.Id == id);
            _context.RdvQueries.Remove(command);
            _context.SaveChanges(User.GetUserId());
            return RedirectToAction("Index");
        }
        public IActionResult CGV()
        {
            return View();
        }
    }
}
