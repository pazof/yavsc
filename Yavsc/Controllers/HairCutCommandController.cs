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
using Yavsc.Models.Google.Messaging;
using Yavsc.Models.Haircut;
using Yavsc.Models.Relationship;
using Yavsc.Services;

namespace Yavsc.Controllers
{
    public class HairCutCommandController : CommandController
    {
        public HairCutCommandController(ApplicationDbContext context, 
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
           
        }
        
        [HttpPost, Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHairCutQuery(HairCutQuery command)
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

                _context.HairCutQueries.Add(command, GraphBehavior.IncludeDependents);
                _context.SaveChanges(User.GetUserId());

                var yaev = command.CreateEvent(_localizer);
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
            return View(command);
       
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

                var yaev = command.CreateEvent(_localizer);
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
            return View(command);
        }
    }
}