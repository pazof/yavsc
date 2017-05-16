
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Yavsc.Models;
using Yavsc.Services;
using Yavsc.ViewModels.Manage;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using Yavsc.Helpers;
using Yavsc.ViewModels.Calendar;
using System.Net;
using Microsoft.Extensions.Localization;
using Yavsc.Models.Workflow;
using Yavsc.Models.Identity;

namespace Yavsc.Controllers
{
    using Models.Relationship;
    using PayPal.Api;
    using Yavsc.Models.Bank;

    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private SiteSettings _siteSettings;

        private ApplicationDbContext _dbContext;

        private GoogleAuthSettings _googleSettings;

        private PayPalSettings _payPalSettings;
        private IGoogleCloudMessageSender _GCMSender;
        private SIRENChecker _cchecker;
        private IStringLocalizer _SR;
        private CompanyInfoSettings _cinfoSettings;

        public ManageController(
            ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender,
        IGoogleCloudMessageSender GCMSender,
        IOptions<SiteSettings> siteSettings,
        IOptions<GoogleAuthSettings> googleSettings,
        IOptions<PayPalSettings> paypalSettings,
        IOptions<CompanyInfoSettings> cinfoSettings,
        IStringLocalizer<Yavsc.Resources.YavscLocalisation> SR,
        ILoggerFactory loggerFactory)
        {
            _dbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _GCMSender = GCMSender;
            _siteSettings = siteSettings.Value;
            _googleSettings = googleSettings.Value;
            _payPalSettings = paypalSettings.Value;
            _cinfoSettings = cinfoSettings.Value;
            _cchecker = new SIRENChecker(cinfoSettings.Value);
            _SR = SR;
            _logger = loggerFactory.CreateLogger<ManageController>();
        }

        //
        // GET: /Manage/Index
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? _SR["Your password has been changed."]
                : message == ManageMessageId.SetPasswordSuccess ? _SR["Your password has been set."]
                : message == ManageMessageId.SetTwoFactorSuccess ? _SR["Your two-factor authentication provider has been set."]
                : message == ManageMessageId.Error ? _SR["An error has occurred."]
                : message == ManageMessageId.AddPhoneSuccess ? _SR["Your phone number was added."]
                : message == ManageMessageId.RemovePhoneSuccess ? _SR["Your phone number was removed."]
                : message == ManageMessageId.ChangeNameSuccess ? _SR["Your name was updated."]
                : message == ManageMessageId.SetActivitySuccess ? _SR["Your activity was set."]
                : message == ManageMessageId.AvatarUpdateSuccess ? _SR["Your avatar was updated."]
                : message == ManageMessageId.IdentityUpdateSuccess ? _SR["Your identity was updated."]
                : message == ManageMessageId.SetBankInfoSuccess ? _SR["Vos informations bancaires ont bien été enregistrées."]
                : message == ManageMessageId.SetAddressSuccess ? _SR["Votre adresse a bien été enregistrée."]
                : "";

            var user = await GetCurrentUserAsync();
            long pc = _dbContext.Blogspot.Count(x => x.AuthorId == user.Id);

            var model = new IndexViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user),
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                UserName = user.UserName,
                PostsCounter = pc,
                Balance = user.AccountBalance,
                ActiveCommandCount = _dbContext.RdvQueries.Count(x => (x.ClientId == user.Id) && (x.EventDate > DateTime.Now)),
                HasDedicatedCalendar = !string.IsNullOrEmpty(user.DedicatedGoogleCalendar),
                Roles = await _userManager.GetRolesAsync(user),
                PostalAddress = user.PostalAddress?.Address,
                FullName = user.FullName,
                Avatar = user.Avatar,
                BankInfo = user.BankInfo,
                DiskUsage = user.DiskUsage,
                DiskQuota = user.DiskQuota
            };
            model.HaveProfessionalSettings = _dbContext.Performers.Any(x => x.PerformerId == user.Id);
            var usrActs = _dbContext.UserActivities.Include(a=>a.Does).Where(a=> a.UserId == user.Id);

            model.HaveActivityToConfigure = usrActs.Any( a=> a.Settings == null && a.Does.SettingsClassName!=null );
            model.Activity = _dbContext.UserActivities.Include(a=>a.Does).Where(u=>u.UserId == user.Id).ToList();
            return View(model);
        }


        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
        {
            ManageMessageId? message = ManageMessageId.Error;
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    message = ManageMessageId.RemoveLoginSuccess;
                }
            }
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public IActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var user = await GetCurrentUserAsync();
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
            // TODO         await _smsSender.SendSmsAsync(_twilioSettings, model.PhoneNumber, "Your security code is: " + code);

            return RedirectToAction(nameof(VerifyPhoneNumber), new { PhoneNumber = model.PhoneNumber });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(1, "User enabled two-factor authentication.");
            }
            return RedirectToAction(nameof(Index), "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(2, "User disabled two-factor authentication.");
            }
            return RedirectToAction(nameof(Index), "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        [HttpGet]
        public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(await GetCurrentUserAsync(), phoneNumber);
            // Send an SMS to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.AddPhoneSuccess });
                }
            }
            // If we got this far, something failed, redisplay the form
            ModelState.AddModelError(string.Empty, "Failed to verify phone number");
            return View(model);
        }

        //
        // GET: /Manage/RemovePhoneNumber
        [HttpGet]
        public async Task<IActionResult> RemovePhoneNumber()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.SetPhoneNumberAsync(user, null);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.RemovePhoneSuccess });
                }
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AddMobileApp(GoogleCloudMobileDeclaration model)
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SetGoogleCalendar(string returnUrl)
        {
            var credential = await _userManager.GetCredentialForGoogleApiAsync(
                _dbContext, User.GetUserId());
            if (credential == null)
                return RedirectToAction("LinkLogin", new { provider = "Google" });

            try
            {
                ViewBag.Calendars = new GoogleApis.CalendarApi(_googleSettings.ApiKey)
                .GetCalendars(credential);
            }
            catch (WebException ex)
            {
                // a bug
                _logger.LogError("Google token, an Forbidden calendar");
                if (ex.HResult == (int)HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("LinkLogin", new { provider = "Google" });
                }
            }
            return View(new SetGoogleCalendarViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetGoogleCalendar(SetGoogleCalendarViewModel model)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == User.GetUserId());
            user.DedicatedGoogleCalendar = model.GoogleCalendarId;
            await _dbContext.SaveChangesAsync(User.GetUserId());
            if (string.IsNullOrEmpty(model.ReturnUrl))
                return RedirectToAction("Index");
            else return Redirect(model.ReturnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> AddBankInfo()
        {
            var uid = User.GetUserId();
            var user = await _dbContext.Users.Include(u=>u.BankInfo).SingleAsync(u=>u.Id==uid);

            return View(user.BankInfo);
        }

        [HttpPost]
        public async Task<IActionResult> AddBankInfo (BankIdentity model)
        {
            if (ModelState.IsValid)
            {
                // TODO PostBankInfoRequirement & auth
                var uid = User.GetUserId();
                var user = _dbContext.Users.Include(u=>u.BankInfo)
                    .Single(u=>u.Id == uid);

                if (user.BankInfo != null)
                 {
                     model.Id = user.BankInfo.Id;
                     _dbContext.Entry(user.BankInfo).State = EntityState.Detached;
                     _dbContext.Update(model);
                 }
                 else {
                     user.BankInfo = model;
                     _dbContext.Update(user);
                 }
                 await _dbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.SetBankInfoSuccess });
        }

        [HttpGet]
        public async Task<IActionResult> SetFullName()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            return View(user.FullName);
        }
        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed their password successfully.");
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //
        // GET: /Manage/ChangeUserName
        public IActionResult ChangeUserName()
        {
            return View(new ChangeUserNameViewModel() { NewUserName = User.Identity.Name });
        }

        public IActionResult CHUN()
        {
            return View(new ChangeUserNameViewModel() { NewUserName = User.Identity.Name });
        }

        [HttpPost]
        public async Task<IActionResult> CHUN(ChangeUserNameViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var oldUserName = user.UserName;

                var result = await this._userManager.SetUserNameAsync(user, model.NewUserName);

                if (result.Succeeded)
                {
                    /* Obsolete : files are no more prefixed using the user name.

                    var userdirinfo = new DirectoryInfo(
                       Path.Combine(_siteSettings.UserFiles.DirName,
                        oldUserName));
                    var newdir = Path.Combine(_siteSettings.UserFiles.DirName,
                       model.NewUserName);
                    if (userdirinfo.Exists)
                        userdirinfo.MoveTo(newdir);
                    */

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed his user name successfully.");
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangeNameSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //
        // GET: /Manage/SetPassword
        [HttpGet]
        public IActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //GET: /Manage/ManageLogins
        [HttpGet]
        public async Task<IActionResult> ManageLogins(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.AddLoginSuccess ? "The external login was added."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await _userManager.GetLoginsAsync(user);
            var otherLogins = _signInManager.GetExternalAuthenticationSchemes().Where(auth => userLogins.All(ul => auth.AuthenticationScheme != ul.LoginProvider)).ToList();
            ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;

            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action("LinkLoginCallback", "Manage");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, User.GetUserId());
            return new ChallengeResult(provider, properties);
        }

        //
        // GET: /Manage/LinkLoginCallback
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var info = await _signInManager.GetExternalLoginInfoAsync(User.GetUserId());
            if (info == null)
            {
                return RedirectToAction(nameof(ManageLogins), new { Message = ManageMessageId.Error });
            }
            var result = await _userManager.AddLoginAsync(user, info);
            var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        [HttpGet]
        public IActionResult SetAvatar()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SetActivity()
        {
            var user = GetCurrentUserAsync().Result;
            var uid = user.Id;
            var existing = _dbContext.Performers
            .Include(p=>p.Performer)
            .Include(x => x.OrganizationAddress)
            .Include(p=>p.Activity)
            .FirstOrDefault(x => x.PerformerId == uid);

            ViewBag.GoogleSettings = _googleSettings;
            if (existing!=null)
            {
                var currentProfile = _dbContext.Performers.Include(x => x.OrganizationAddress)
                .First(x => x.PerformerId == uid);
                ViewBag.Activities = _dbContext.ActivityItems(existing.Activity);
                return View(currentProfile);
            }

            ViewBag.Activities = _dbContext.ActivityItems(new List<UserActivity>());
            return View(new PerformerProfile
            {
                PerformerId = user.Id,
                Performer = user,
                OrganizationAddress = new Location()
            });
        }


        [HttpPost]
        public async Task<IActionResult> SetActivity(PerformerProfile model)
        {
            var user = GetCurrentUserAsync().Result;
            var uid = user.Id;
            try
            {
                if (ModelState.IsValid)
                {

                    var exSiren = await _dbContext.ExceptionsSIREN.FirstOrDefaultAsync(
                        ex => ex.SIREN == model.SIREN
                    );
                    if (exSiren != null)
                    {
                        _logger.LogInformation("Exception SIREN:" + exSiren);
                    }
                    else
                    {
                        var taskCheck = await _cchecker.CheckAsync(model.SIREN);
                        if (!taskCheck.success)
                        {
                            ModelState.AddModelError(
                                "SIREN",
                                _SR["Invalid company number"] + " (" + taskCheck.errorCode + ")"
                            );
                            _logger.LogInformation($"Invalid company number: {model.SIREN}/{taskCheck.errorType}/{taskCheck.errorCode}/{taskCheck.errorMessage}" );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("SIREN", ex.Message);
            }
            if (ModelState.IsValid)
            {
                if (uid == model.PerformerId)
                {
                    bool addrexists = _dbContext.Map.Any(x => model.OrganizationAddress.Id == x.Id);
                    if (!addrexists)
                    {
                        _dbContext.Map.Add(model.OrganizationAddress);
                    }

                    if (_dbContext.Performers.Any(p=>p.PerformerId == uid))
                    {
                        _dbContext.Update(model);
                    }
                    else _dbContext.Performers.Add(model);
                    _dbContext.SaveChanges(User.GetUserId());
                    // Give this user the Performer role
                    if (!User.IsInRole("Performer"))
                        await _userManager.AddToRoleAsync(user, "Performer");
                    var message = ManageMessageId.SetActivitySuccess;

                    return RedirectToAction(nameof(Index), new { Message = message });

                }
                else ModelState.AddModelError(string.Empty, $"Access denied ({uid} vs {model.PerformerId})");
            }
            ViewBag.Activities = _dbContext.ActivityItems(new List<UserActivity>());
            ViewBag.GoogleSettings = _googleSettings;
            model.Performer = _dbContext.Users.Single(u=>u.Id == model.PerformerId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UnsetActivity()
        {
            var user = GetCurrentUserAsync().Result;
            var uid = user.Id;
            bool existing = _dbContext.Performers.Any(x => x.PerformerId == uid);
            if (existing)
            {
                _dbContext.Performers.Remove(
                    _dbContext.Performers.First(x => x.PerformerId == uid)
                );
                _dbContext.SaveChanges(User.GetUserId());
                await _userManager.RemoveFromRoleAsync(user, "Performer");
            }
            var message = ManageMessageId.UnsetActivitySuccess;
            return RedirectToAction(nameof(Index), new { Message = message });
        }

        [HttpGet, Route("/Manage/Credits")]
        public IActionResult Credits()
        {

            return View();
        }

        public IActionResult Credit(string id)
        {
            if (id == "Cancel" || id == "Return")
                {
                    return View ("Credit"+id);
                }
            return View();
        }


        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            ChangeNameSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            SetActivitySuccess,
            UnsetActivitySuccess,
            AvatarUpdateSuccess,
            IdentityUpdateSuccess,
            SetBankInfoSuccess,

            SetAddressSuccess,
            Error
        }


        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.FindByIdAsync(HttpContext.User.GetUserId());
        }

        #endregion

        [HttpGet]
        public async Task <IActionResult> SetAddress()
        {
            var uid = User.GetUserId();
            var user = await _dbContext.Users.Include(u=>u.PostalAddress).SingleAsync(u=>u.Id==uid);
            ViewBag.GoogleSettings =  _googleSettings;
            return View(user.PostalAddress);
        }

        [HttpPost]
        public async Task <IActionResult> SetAddress(Location model)
        {
            if (ModelState.IsValid) {
                var uid = User.GetUserId();

                var user = _dbContext.Users.Include(u=>u.PostalAddress).Single(u=>u.Id==uid);

                var existingLocation = _dbContext.Locations.FirstOrDefault( x=>x.Address == model.Address
                && x.Longitude == model.Longitude && x.Latitude == model.Latitude );

                if (existingLocation!=null) {
                    user.PostalAddressId = existingLocation.Id;
                } else _dbContext.Attach<Location>(model);
                user.PostalAddress = model;
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { Message = ManageMessageId.SetAddressSuccess });
            }
            ViewBag.GoogleSettings = _googleSettings;
            return View(model);
        }

        public IActionResult PaymentInfo(string id)
        {
            var context = PayPalHelpers.CreateAPIContext();
            var payment = Payment.Get(context,id);
            return View (payment);
        }

    }
}
