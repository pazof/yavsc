
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
using PayPal.PayPalAPIInterfaceService;
using System.Collections.Generic;
using Yavsc.Helpers;
using Yavsc.ViewModels.Calendar;
using System.Net;
using Microsoft.Extensions.Localization;
using Yavsc.Models.Workflow;
using Yavsc.Models.Identity;

namespace Yavsc.Controllers
{
    [Authorize, ServiceFilter(typeof(LanguageActionFilter))]
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
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.ChangeNameSuccess ? "Your name was updated."
                : message == ManageMessageId.SetActivitySuccess ? "Your activity was set."
                : message == ManageMessageId.AvatarUpdateSuccess ? "Your avatar was updated."
                : message == ManageMessageId.IdentityUpdateSuccess ? "Your identity was updated."
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
                ActiveCommandCount = _dbContext.BookQueries.Count(x => (x.ClientId == user.Id) && (x.EventDate > DateTime.Now)),
                HasDedicatedCalendar = !string.IsNullOrEmpty(user.DedicatedGoogleCalendar),
                Roles = await _userManager.GetRolesAsync(user),
                PostalAddress = user.PostalAddress?.Address,
                FullName = user.FullName,
                Avatar = user.Avatar,
                BankInfo = user.BankInfo
            };
            if (_dbContext.Performers.Any(x => x.PerformerId == user.Id))
            {
                var code = _dbContext.Performers.First(x => x.PerformerId == user.Id).ActivityCode;
                model.Activity = _dbContext.Activities.First(x => x.Code == code);
            }
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

        [HttpGet, Authorize]
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

        [HttpPost, ValidateAntiForgeryToken,
        Authorize]
        public async Task<IActionResult> SetGoogleCalendar(SetGoogleCalendarViewModel model)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == User.GetUserId());
            user.DedicatedGoogleCalendar = model.GoogleCalendarId;
            await _dbContext.SaveChangesAsync();
            if (string.IsNullOrEmpty(model.ReturnUrl))
                return RedirectToAction("Index");
            else return Redirect(model.ReturnUrl);
        }

        [HttpGet,Authorize]
        public async Task<IActionResult> AddBankInfo()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());

            return View(new AddBankInfoViewModel(
                user.BankInfo));
        }

        [HttpGet,Authorize]
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

        [HttpGet, Authorize]
        public IActionResult SetAvatar()
        {
            throw new NotImplementedException();
        }

        [HttpGet, Authorize]
        public IActionResult SetActivity()
        {
            var user = GetCurrentUserAsync().Result;
            var uid = user.Id;
            bool existing = _dbContext.Performers.Any(x => x.PerformerId == uid);
            ViewBag.Activities = _dbContext.ActivityItems(null);
            ViewBag.GoogleSettings = _googleSettings;
            if (existing)
            {
                var currentProfile = _dbContext.Performers.Include(x => x.OrganizationAddress)
                .First(x => x.PerformerId == uid);
                string currentCode = currentProfile.ActivityCode;
                return View(currentProfile);
            }
            return View(new PerformerProfile
            {
                PerformerId = user.Id,
                OrganizationAddress = new Location()
            });
        }


        [HttpPost]
        [Authorize]
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
                            _logger.LogInformation("Invalid company number, using key:" + _cinfoSettings.ApiKey);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                    bool existing = _dbContext.Performers.Any(x => x.PerformerId == uid);
                    if (existing)
                    {
                        _dbContext.Update(model);
                    }
                    else _dbContext.Performers.Add(model);
                    _dbContext.SaveChanges();

                    // Give this user the Performer role
                    if (!User.IsInRole("Performer"))
                        await _userManager.AddToRoleAsync(user, "Performer");
                    var message = ManageMessageId.SetActivitySuccess;

                    return RedirectToAction(nameof(Index), new { Message = message });

                }
                else ModelState.AddModelError(string.Empty, $"Access denied ({uid} vs {model.PerformerId})");
            }
            ViewBag.GoogleSettings = _googleSettings;
            ViewBag.Activities = _dbContext.ActivityItems(model.ActivityCode);
            return View(model);
        }

        [HttpPost, Authorize]
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
                _dbContext.SaveChanges();
                await _userManager.RemoveFromRoleAsync(user, "Performer");
            }
            var message = ManageMessageId.UnsetActivitySuccess;
            return RedirectToAction(nameof(Index), new { Message = message });
        }

        [HttpGet, Route("/Manage/Credits")]
        public IActionResult Credits()
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            config.Add("mode", "sandbox");
            config.Add("account1.apiUsername", _payPalSettings.UserId);
            config.Add("account1.apiPassword", _payPalSettings.Secret);
            config.Add("account1.apiSignature", _payPalSettings.Signature);
            PayPalAPIInterfaceServiceService s = new PayPalAPIInterfaceServiceService(config);

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
            Error
        }


        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.FindByIdAsync(HttpContext.User.GetUserId());
        }

        #endregion
    }
}
