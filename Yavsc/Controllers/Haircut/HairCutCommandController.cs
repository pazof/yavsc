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
    using Yavsc.Helpers;
    using Yavsc.Models;
    using Yavsc.Models.Google.Messaging;
    using Yavsc.Models.Relationship;
    using Yavsc.Services;
    using Newtonsoft.Json;
    using Microsoft.AspNet.Http;
    using Yavsc.Extensions;
    using Yavsc.Models.Haircut;
    using System.Globalization;
    using Microsoft.AspNet.Mvc.Rendering;
    using System.Collections.Generic;
    using PayPal.Api;

    public class HairCutCommandController : CommandController
    {
        public HairCutCommandController(ApplicationDbContext context,
          IOptions<PayPalSettings> payPalSettings,
        IOptions<GoogleAuthSettings> googleSettings,
        IGoogleCloudMessageSender GCMSender,
          UserManager<ApplicationUser> userManager,
          IStringLocalizer<Yavsc.Resources.YavscLocalisation> localizer,
          IEmailSender emailSender,
          IOptions<SmtpSettings> smtpSettings,
          IOptions<SiteSettings> siteSettings,
          ILoggerFactory loggerFactory) : base(context,googleSettings,GCMSender,userManager,
          localizer,emailSender,smtpSettings,siteSettings,loggerFactory)
        {
            this.payPalSettings = payPalSettings.Value;
        }
        PayPalSettings payPalSettings;

        private async Task<HairCutQuery> GetQuery(long id)
        {
            return await _context.HairCutQueries
            .Include(x => x.Location)
            .Include(x => x.PerformerProfile)
            .Include(x => x.Prestation)
            .Include(x => x.PerformerProfile.Performer)
            .Include(x => x.Regularisation)
            .SingleAsync(m => m.Id == id);
        }
        public async Task<IActionResult> ClientCancel(long id)
        {
            HairCutQuery command = await GetQuery(id);
            if (command == null)
            {
                return HttpNotFound();
            }
            return View (command);
        }
        public async Task<IActionResult> ClientCancelConfirm(long id)
        {
            var query = await GetQuery(id);if (query == null)
            {
                return HttpNotFound();
            }
            var uid = User.GetUserId();
            if (query.ClientId!=uid)
                return new ChallengeResult();
            _context.HairCutQueries.Remove(query);
            await _context.SaveChangesAsync();
            return await Index();
        }
        /// <summary>
        /// List client's queries
        /// </summary>
        /// <returns></returns>
        public override async Task<IActionResult> Index()
        {
            var uid = User.GetUserId();
            return View("Index", await _context.HairCutQueries
            .Include(x => x.Client)
            .Include(x => x.PerformerProfile)
            .Include(x => x.PerformerProfile.Performer)
            .Include(x => x.Location)
            .Where(x=> x.ClientId == uid || x.PerformerId == uid)
            .ToListAsync());
        }

        public async Task<IActionResult> Details(long id, string paymentId, string token, string PayerID)
        {

            HairCutQuery command = await _context.HairCutQueries
            .Include(x => x.Location)
            .Include(x => x.PerformerProfile)
            .Include(x => x.Prestation)
            .Include(x => x.PerformerProfile.Performer)
            .Include(x => x.Regularisation)
            .SingleAsync(m => m.Id == id);
            if (command == null)
            {
                return HttpNotFound();
            }

            return View(command);
        }

        public async Task<IActionResult> Execute(long id, string paymentId, string token, string PayerID)
        {
            HairCutQuery command = await _context.HairCutQueries
            .Include(x => x.Location)
            .Include(x => x.PerformerProfile)
            .Include(x => x.Prestation)
            .Include(x => x.PerformerProfile.Performer)
            .Include(x => x.Regularisation)
            .SingleAsync(m => m.Id == id);

            if (command == null)
            {
                return HttpNotFound();
            }
             var context =  payPalSettings.CreateAPIContext();
            var payment = Payment.Get(context,paymentId);

            var execution = new PaymentExecution{ transactions = payment.transactions,
            payer_id = PayerID };
            var result = payment.Execute(context,execution);

            return View(command);
        }
        /// <summary>
        /// Crée une requête en coiffure à domicile
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <param name="taintIds"></param>
        /// <returns></returns>
        [HttpPost, Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHairCutQuery(HairCutQuery model, string taintIds)
        {
            // TODO utiliser Markdown-av+tags
            var uid = User.GetUserId();
            var prid = model.PerformerId;
            long[] longtaintIds = null;
            List<HairTaint> colors = null;

            if (taintIds!=null) {
                longtaintIds = taintIds.Split(',').Select(s=>long.Parse(s)).ToArray();
                colors = _context.HairTaint.Where(t=> longtaintIds.Contains(t.Id)).ToList();
                // a Prestation is required
                model.Prestation.Taints = colors.Select(c =>
                    new HairTaintInstance { Taint = c }).ToList();
            }
            if (string.IsNullOrWhiteSpace(uid)
            || string.IsNullOrWhiteSpace(prid))
                throw new InvalidOperationException(
                    "This method needs a PerformerId"
                );
            var pro = _context.Performers.Include(
                u => u.Performer
            ).Include(u => u.Performer.Devices)
            .FirstOrDefault(
                x => x.PerformerId == model.PerformerId
            );
            model.PerformerProfile = pro;
            // FIXME Why!!
            // ModelState.ClearValidationState("PerformerProfile.Avatar");
            // ModelState.ClearValidationState("Client.Avatar");
            // ModelState.ClearValidationState("ClientId");

            if (ModelState.IsValid)
                {
                    if (model.Location!=null) {
                        var existingLocation = await _context.Locations.FirstOrDefaultAsync( x=>x.Address == model.Location.Address
                        && x.Longitude == model.Location.Longitude && x.Latitude == model.Location.Latitude );

                        if (existingLocation!=null) {
                            model.Location=existingLocation;
                        }
                        else _context.Attach<Location>(model.Location);
                    }
                    var existingPrestation = await _context.HairPrestation.FirstOrDefaultAsync( x=> model.PrestationId == x.Id );

                    if (existingPrestation!=null) {
                        model.Prestation = existingPrestation;
                    }
                    else _context.Attach<HairPrestation>(model.Prestation);

                _context.HairCutQueries.Add(model);
                await _context.SaveChangesAsync(uid);
                var brusherProfile = await _context.BrusherProfile.SingleAsync(p=>p.UserId == pro.PerformerId);
                model.Client = await  _context.Users.SingleAsync(u=>u.Id == model.ClientId);
                model.SelectedProfile = brusherProfile;
                var yaev = model.CreateEvent(_localizer, brusherProfile);
                MessageWithPayloadResponse grep = null;

                if (pro.AcceptPublicContact)
                {
                    if (pro.AcceptNotifications) {
                        if (pro.Performer.Devices.Count > 0) {
                            var regids = model.PerformerProfile.Performer
                            .Devices.Select(d => d.GCMRegistrationId);
                            grep = await _GCMSender.NotifyHairCutQueryAsync(_googleSettings,regids,yaev);
                        }
                        // TODO setup a profile choice to allow notifications
                        // both on mailbox and mobile
                        // if (grep==null || grep.success<=0 || grep.failure>0)
                        ViewBag.GooglePayload=grep;
                        if (grep!=null)
                        _logger.LogWarning($"Performer: {model.PerformerProfile.Performer.UserName} success: {grep.success} failure: {grep.failure}");
                    }

                    await _emailSender.SendEmailAsync(
                        _siteSettings, _smtpSettings,
                        model.PerformerProfile.Performer.Email,
                        yaev.Topic+" "+yaev.Sender,
                        $"{yaev.Message}\r\n-- \r\n{yaev.Previsional}\r\n{yaev.EventDate}\r\n"
                    );
                }
                else {
                    // TODO if (AcceptProContact) try & find a bookmaker to send him this query
                }
                ViewBag.Activity =  _context.Activities.FirstOrDefault(a=>a.Code == model.ActivityCode);
                ViewBag.GoogleSettings = _googleSettings;
                var items = model.GetBillItems();
                var addition = items.Addition();
                ViewBag.Addition = addition.ToString("C",CultureInfo.CurrentUICulture);

                return View("CommandConfirmation",model);
            }
            ViewBag.Activity =  _context.Activities.FirstOrDefault(a=>a.Code == model.ActivityCode);
            ViewBag.GoogleSettings = _googleSettings;
            SetViewData(model.ActivityCode,model.PerformerId,model.Prestation);
            return View("HairCut",model);
        }

        public async Task<ActionResult> HairCut(string performerId, string activityCode)
        {
            HairPrestation pPrestation=null;
            var prestaJson = HttpContext.Session.GetString("HairCutPresta") ;
            if (prestaJson!=null) {
                pPrestation = JsonConvert.DeserializeObject<HairPrestation>(prestaJson);
            }
            else {
                pPrestation = new HairPrestation {};
            }

            var uid = User.GetUserId();
            var user = await _userManager.FindByIdAsync(uid);

            SetViewData(activityCode,performerId,pPrestation);

            var perfer = _context.Performers.Include(
                    p=>p.Performer
                ).Single(p=>p.PerformerId == performerId);
            var result = new HairCutQuery {
                PerformerProfile = perfer,
                PerformerId = perfer.PerformerId,
                ClientId = uid,
                Prestation = pPrestation,
                Client = user
            };

            return View(result);
        }
        private void SetViewData (string activityCode, string performerId, HairPrestation pPrestation )
        {
            ViewBag.HairTaints = _context.HairTaint.Include(t=>t.Color);
            ViewBag.HairTaintsItems = _context.HairTaint.Include(t=>t.Color).Select(
                c=>
                new SelectListItem {
                    Text = c.Color.Name+" "+c.Brand,
                    Value = c.Id.ToString()
                 }
            );
            ViewBag.HairTechnos = EnumExtensions.GetSelectList(typeof(HairTechnos),_localizer);
            ViewBag.HairLength = EnumExtensions.GetSelectList(typeof(HairLength),_localizer);
            ViewBag.Activity = _context.Activities.First(a => a.Code == activityCode);
            ViewBag.Gender = EnumExtensions.GetSelectList(typeof(HairCutGenders),_localizer,HairCutGenders.Women);
            ViewBag.HairDressings = EnumExtensions.GetSelectList(typeof(HairDressings),_localizer);
            ViewBag.ColorsClass = ( pPrestation.Tech == HairTechnos.Color
            || pPrestation.Tech == HairTechnos.Mech ) ? "":"hidden";
            ViewBag.TechClass = ( pPrestation.Gender == HairCutGenders.Women ) ? "":"hidden";
            ViewData["PerfPrefs"] = _context.BrusherProfile.Single(p=>p.UserId == performerId);
        }

        [HttpPost, Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHairMultiCutQuery(HairMultiCutQuery command)
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

                _context.HairMultiCutQueries.Add(command, GraphBehavior.IncludeDependents);
                _context.SaveChanges(User.GetUserId());
                var brSettings = await _context.BrusherProfile.SingleAsync(
                    bp=>bp.UserId == command.PerformerId
                );
                var yaev = command.CreateEvent(_localizer,brSettings);
                MessageWithPayloadResponse grep = null;

                if (pro.AcceptNotifications
                && pro.AcceptPublicContact)
                {
                    if (pro.Performer.Devices.Count > 0) {
                        var regids = command.PerformerProfile.Performer
                        .Devices.Select(d => d.GCMRegistrationId);
                        grep = await _GCMSender.NotifyHairCutQueryAsync(_googleSettings,regids,yaev);
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
                        yaev.Topic+" "+yaev.Sender,
                        $"{yaev.Message}\r\n-- \r\n{yaev.Previsional}\r\n{yaev.EventDate}\r\n"
                    );
                }
                ViewBag.Activity =  _context.Activities.FirstOrDefault(a=>a.Code == command.ActivityCode);
                ViewBag.GoogleSettings = _googleSettings;
                return View("CommandConfirmation",command);
            }
            ViewBag.Activity =  _context.Activities.FirstOrDefault(a=>a.Code == command.ActivityCode);
            ViewBag.GoogleSettings = _googleSettings;
            return View("HairCut",command);
        }
    }
}
