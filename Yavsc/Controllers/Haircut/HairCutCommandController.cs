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
    using Yavsc.Models.Messaging;
    using PayPal.PayPalAPIInterfaceService.Model;

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
          ICalendarManager calManager,
          ILoggerFactory loggerFactory) : base(context, googleSettings, GCMSender, userManager,
          calManager, localizer, emailSender, smtpSettings, siteSettings, loggerFactory)
        {
            this.payPalSettings = payPalSettings.Value;
        }
        PayPalSettings payPalSettings;
        
        private async Task<HairCutQuery> GetQuery(long id)
        {
            var query = await _context.HairCutQueries
            .Include(x => x.Location)
            .Include(x => x.PerformerProfile)
            .Include(x => x.Prestation)
            .Include(x => x.PerformerProfile.Performer)
            .Include(x => x.PerformerProfile.Performer.Devices)
            .Include(x => x.Regularisation)
            .SingleAsync(m => m.Id == id);
            query.SelectedProfile = await _context.BrusherProfile.SingleAsync(b => b.UserId == query.PerformerId);
            return query;
        }
        public async Task<IActionResult> ClientCancel(long id)
        {
            HairCutQuery command = await GetQuery(id);
            if (command == null)
            {
                return HttpNotFound();
            }
            SetViewBagPaymentUrls(id);
            return View(command);
        }
        public async Task<IActionResult> PaymentConfirmation([FromRoute] long id, string token, string PayerID)
        {
            HairCutQuery command = await GetQuery(id);
            if (command == null)
            {
                return HttpNotFound();
            }
            var paymentInfo = await _context.ConfirmPayment(User.GetUserId(), PayerID, token);
            ViewData["paymentinfo"] = paymentInfo;
            command.Regularisation = paymentInfo.DbContent;
            command.PaymentId = token;
            bool paymentOk = false;
            if (paymentInfo.DetailsFromPayPal != null)
                if (paymentInfo.DetailsFromPayPal.Ack == AckCodeType.SUCCESS) 
                {
                    // FIXME Assert (command.ValidationDate == null)
                    if (command.ValidationDate == null) {
                        paymentOk = true;
                        command.ValidationDate = DateTime.Now;
                    }
                }
            await _context.SaveChangesAsync(User.GetUserId());
            SetViewBagPaymentUrls(id);
            if (command.PerformerProfile.AcceptPublicContact && paymentOk)
            {
                MessageWithPayloadResponse grep = null;
                var yaev = command.CreatePaymentEvent(paymentInfo,  _localizer);
                if (command.PerformerProfile.AcceptNotifications)
                {
                    if (command.PerformerProfile.Performer.Devices.Count > 0)
                    {
                        var regids = command.PerformerProfile.Performer
                        .Devices.Select(d => d.GCMRegistrationId);
                        
                        grep = await _GCMSender.NotifyAsync(_googleSettings, regids, yaev);
                    }
                    // TODO setup a profile choice to allow notifications
                    // both on mailbox and mobile
                    // if (grep==null || grep.success<=0 || grep.failure>0)
                    ViewBag.GooglePayload = grep;
                }

                ViewBag.EmailSent = await _emailSender.SendEmailAsync(
                    _siteSettings, _smtpSettings,
                    command.PerformerProfile.Performer.UserName,
                    command.PerformerProfile.Performer.Email,
                    yaev.Topic,
                    yaev.CreateBody()
                );
            }
            else
            {
                // TODO if (AcceptProContact) try & find a bookmaker to send him this query
            }

            ViewData["Notify"] = new List<Notification> {
                new Notification {
                    title= "Paiment PayPal",
                    body = "Votre paiment a été accépté."
                }
            };
            return View("Details", command);
        }

        private void SetViewBagPaymentUrls(long id)
        {
            ViewBag.CreatePaymentUrl = Request.ToAbsolute("api/haircut/createpayment/" + id);
            ViewBag.ExecutePaymentUrl = Request.ToAbsolute("api/payment/execute");
            ViewBag.Urls = Request.GetPaymentUrls("HairCutCommand", id.ToString());
        }
        public async Task<IActionResult> ClientCancelConfirm(long id)
        {
            var query = await GetQuery(id); if (query == null)
            {
                return HttpNotFound();
            }
            var uid = User.GetUserId();
            if (query.ClientId != uid)
                return new ChallengeResult();
            _context.HairCutQueries.Remove(query);
            await _context.SaveChangesAsync();
            return await Index();
        }
        /// <summary>
        /// List client's queries (and only client's ones)
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
            .Where(x => x.ClientId == uid)
            .ToListAsync());
        }

        public override async Task<IActionResult> Details(long id)
        {
            HairCutQuery command = await _context.HairCutQueries
            .Include(x => x.Location)
            .Include(x => x.PerformerProfile)
            .Include(x => x.Prestation)
            .Include(x => x.PerformerProfile.Performer)
            .Include(x => x.Regularisation)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (command == null)
            {
                return HttpNotFound();
            }
            SetViewBagPaymentUrls(id);
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
            model.ClientId = uid;

            var prid = model.PerformerId;
            var brusherProfile = await _context.BrusherProfile.SingleAsync(p => p.UserId == prid);
            long[] longtaintIds = null;
            List<HairTaint> colors = null;

            if (string.IsNullOrWhiteSpace(uid)
            || string.IsNullOrWhiteSpace(prid))
                throw new InvalidOperationException(
                    "This method needs a PerformerId"
                );


            if (!model.Consent)
                ModelState.AddModelError("Consent", "Vous devez accepter les conditions générales de vente de ce service");

            if (ModelState.IsValid)
            {
                _logger.LogInformation("le Model _est_ valide.");
                var pro = _context.Performers.Include(
                     u => u.Performer
                 ).Include(u => u.Performer.Devices)
                 .FirstOrDefault(
                     x => x.PerformerId == model.PerformerId
                 );
              

                  if (taintIds != null)
                  {
                      longtaintIds = taintIds.Split(',').Select(s => long.Parse(s)).ToArray();
                      colors = _context.HairTaint.Where(t => longtaintIds.Contains(t.Id)).ToList();
                      // a Prestation is required
                      model.Prestation.Taints = colors.Select(c =>
                          new HairTaintInstance { Taint = c }).ToList();
                  }

                  // Une prestation pour enfant ou homme inclut toujours la coupe.
                  if (model.Prestation.Gender != HairCutGenders.Women)
                      model.Prestation.Cut = true;
                  if (model.Location != null)
                  {
                      var existingLocation = await _context.Locations.FirstOrDefaultAsync(x => x.Address == model.Location.Address
                     && x.Longitude == model.Location.Longitude && x.Latitude == model.Location.Latitude);

                      if (existingLocation != null)
                      {
                          model.Location = existingLocation;
                      }
                      else _context.Attach<Location>(model.Location);
                  }
                  var existingPrestation = await _context.HairPrestation.FirstOrDefaultAsync(x => model.PrestationId == x.Id);

                  if (existingPrestation != null)
                  {
                      model.Prestation = existingPrestation;
                  }
                  else _context.Attach<HairPrestation>(model.Prestation);

                  _context.HairCutQueries.Add(model);

                await _context.SaveChangesAsync(uid);
                _logger.LogInformation("la donnée _est_ sauvée:");
                MessageWithPayloadResponse grep = null;
                model.SelectedProfile = brusherProfile;
                model.Client = await _userManager.FindByIdAsync(uid);
                _logger.LogInformation(JsonConvert.SerializeObject(model));
                var yaev = model.CreateNewHairCutQueryEvent(_localizer);

                if (pro.AcceptPublicContact)
                {
                    if (pro.AcceptNotifications)
                    {
                        if (pro.Performer.Devices.Count > 0)
                        {
                            var regids = pro.Performer.Devices.Select(d => d.GCMRegistrationId);
                            grep = await _GCMSender.NotifyHairCutQueryAsync(_googleSettings, regids, yaev);
                        }
                        // TODO setup a profile choice to allow notifications
                        // both on mailbox and mobile
                        // if (grep==null || grep.success<=0 || grep.failure>0)
                        ViewBag.GooglePayload = grep;
                        if (grep != null)
                            _logger.LogWarning($"Performer: {pro.Performer.UserName} success: {grep.success} failure: {grep.failure}");
                    }
                    // TODO if pro.AllowCalendarEventInsert
                    if (pro.Performer.DedicatedGoogleCalendar != null && yaev.EventDate != null)
                    {
                        _logger.LogInformation("Inserting an event in the calendar");
                        DateTime evdate = yaev.EventDate ?? new DateTime();
                        var result = await _calendarManager.CreateEventAsync(pro.Performer.Id,
                            pro.Performer.DedicatedGoogleCalendar,
                            evdate, 3600, yaev.Topic, yaev.Client.UserName + " : " + yaev.Reason,
                            yaev.Location?.Address, false
                        );
                        if (result.Id == null)
                            _logger.LogWarning("Something went wrong, calendar event not created");
                    }
                    else _logger.LogWarning($"Calendar: {pro.Performer.DedicatedGoogleCalendar != null}\nEventDate: {yaev.EventDate != null}");

                    await _emailSender.SendEmailAsync(
                        _siteSettings, _smtpSettings,
                         pro.Performer.UserName,
                        pro.Performer.Email,
                        $"{yaev.Client.UserName}: {yaev.Reason}",
                        $"{yaev.Reason}\r\n-- \r\n{yaev.Previsional}\r\n{yaev.EventDate}\r\n"
                    );
                }
                else
                {
                    // TODO if (AcceptProContact) try & find a bookmaker to send him this query
                }
                ViewBag.Activity = _context.Activities.FirstOrDefault(a => a.Code == model.ActivityCode);
                ViewBag.GoogleSettings = _googleSettings;
                var items = model.GetBillItems();
                var addition = items.Addition();
                ViewBag.Addition = addition.ToString("C", CultureInfo.CurrentUICulture);
                return View("CommandConfirmation", model);
            }
            ViewBag.Activity = _context.Activities.FirstOrDefault(a => a.Code == model.ActivityCode);
            ViewBag.GoogleSettings = _googleSettings;
            model.SelectedProfile = brusherProfile;

            SetViewData(model.ActivityCode, model.PerformerId, model.Prestation);
            return View("HairCut", model);
        }


        public async Task<ActionResult> HairCut(string performerId, string activityCode)
        {
            HairPrestation pPrestation = null;
            var prestaJson = HttpContext.Session.GetString("HairCutPresta");
            if (prestaJson != null)
            {
                pPrestation = JsonConvert.DeserializeObject<HairPrestation>(prestaJson);
            }
            else
            {
                pPrestation = new HairPrestation { };
            }

            var uid = User.GetUserId();
            var user = await _userManager.FindByIdAsync(uid);

            SetViewData(activityCode, performerId, pPrestation);

            var perfer = _context.Performers.Include(
                    p => p.Performer
                ).Single(p => p.PerformerId == performerId);
            var result = new HairCutQuery
            {
                PerformerProfile = perfer,
                PerformerId = perfer.PerformerId,
                ClientId = uid,
                Prestation = pPrestation,
                Client = user,
                Location = new Location { Address = "" },
                EventDate = new DateTime()
            };
            return View(result);
        }
        private void SetViewData(string activityCode, string performerId, HairPrestation pPrestation)
        {
            ViewBag.HairTaints = _context.HairTaint.Include(t => t.Color);
            ViewBag.HairTaintsItems = _context.HairTaint.Include(t => t.Color).Select(
                c =>
                new SelectListItem
                {
                    Text = c.Color.Name + " " + c.Brand,
                    Value = c.Id.ToString()
                }
            );
            ViewBag.HairTechnos = EnumExtensions.GetSelectList(typeof(HairTechnos), _localizer);
            ViewBag.HairLength = EnumExtensions.GetSelectList(typeof(HairLength), _localizer);
            ViewBag.Activity = _context.Activities.First(a => a.Code == activityCode);
            ViewBag.Gender = EnumExtensions.GetSelectList(typeof(HairCutGenders), _localizer, HairCutGenders.Women);
            ViewBag.HairDressings = EnumExtensions.GetSelectList(typeof(HairDressings), _localizer);
            ViewBag.ColorsClass = (pPrestation.Tech == HairTechnos.Color
            || pPrestation.Tech == HairTechnos.Mech) ? "" : "hidden";
            ViewBag.TechClass = (pPrestation.Gender == HairCutGenders.Women) ? "" : "hidden";
            ViewData["PerfPrefs"] = _context.BrusherProfile.Single(p => p.UserId == performerId);
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
                var existingLocation = _context.Locations.FirstOrDefault(x => x.Address == command.Location.Address
               && x.Longitude == command.Location.Longitude && x.Latitude == command.Location.Latitude);

                if (existingLocation != null)
                {
                    command.Location = existingLocation;
                }
                else _context.Attach<Location>(command.Location);

                _context.HairMultiCutQueries.Add(command, GraphBehavior.IncludeDependents);
                _context.SaveChanges(User.GetUserId());
                var brSettings = await _context.BrusherProfile.SingleAsync(
                    bp => bp.UserId == command.PerformerId
                );
                var yaev = command.CreateEvent(_localizer, brSettings);
                string msg = yaev.CreateBoby();
                MessageWithPayloadResponse grep = null;

                if (pro.AcceptNotifications
                && pro.AcceptPublicContact)
                {
                    if (pro.Performer.Devices?.Count > 0)
                    {
                        var regids = command.PerformerProfile.Performer
                        .Devices.Select(d => d.GCMRegistrationId);
                        grep = await _GCMSender.NotifyHairCutQueryAsync(_googleSettings, regids, yaev);
                    }
                    // TODO setup a profile choice to allow notifications
                    // both on mailbox and mobile, and to allow calendar event insertion.
                    // if (grep==null || grep.success<=0 || grep.failure>0)
                    ViewBag.GooglePayload = grep;
                    if (grep != null)
                        _logger.LogWarning($"Performer: {command.PerformerProfile.Performer.UserName} success: {grep.success} failure: {grep.failure}");


                    if (pro.Performer.DedicatedGoogleCalendar != null && yaev.EventDate != null)
                    {
                        DateTime evdate = yaev.EventDate ?? new DateTime();
                        await _calendarManager.CreateEventAsync(
                            pro.Performer.Id,
                            pro.Performer.DedicatedGoogleCalendar,
                            evdate, 3600, yaev.Topic, msg,
                            yaev.Location?.ToString(), false
                        );
                    }

                    await _emailSender.SendEmailAsync(
                        _siteSettings, _smtpSettings,
                        command.PerformerProfile.Performer.UserName,
                        command.PerformerProfile.Performer.Email,
                        yaev.Topic + " " + yaev.Sender,
                        $"{msg}\r\n-- \r\n{yaev.Previsional}\r\n{yaev.EventDate}\r\n"
                    );
                }
                ViewBag.Activity = _context.Activities.FirstOrDefault(a => a.Code == command.ActivityCode);
                ViewBag.GoogleSettings = _googleSettings;
                return View("CommandConfirmation", command);
            }
            ViewBag.Activity = _context.Activities.FirstOrDefault(a => a.Code == command.ActivityCode);
            ViewBag.GoogleSettings = _googleSettings;
            return View("HairCut", command);
        }
    }
}
