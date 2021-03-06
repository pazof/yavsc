

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Http;
using Yavsc.Models;
using Yavsc.Services;
using Yavsc.ViewModels.Account;
using Microsoft.Extensions.Localization;
using Microsoft.Data.Entity;
using Newtonsoft.Json;

namespace Yavsc.Controllers
{
    using Yavsc.Abstract.Manage;
    using Yavsc.Auth;
    using Yavsc.Helpers;

    public class AccountController : Controller
    {
        private const string nextPageTokenKey = "nextPageTokenKey";
        private const int defaultLen = 10;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        // private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        readonly SiteSettings _siteSettings;
        readonly TwilioSettings _twilioSettings;

        readonly IStringLocalizer _localizer;

        //  TwilioSettings _twilioSettings;

        readonly ApplicationDbContext _dbContext;
        

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IOptions<SiteSettings> siteSettings,
            ILoggerFactory loggerFactory, IOptions<TwilioSettings> twilioSettings,
            IStringLocalizer<Yavsc.YavscLocalisation> localizer,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            var emailUserTokenProvider = new UserTokenProvider();
            _userManager.RegisterTokenProvider("EmailConfirmation", emailUserTokenProvider);
            _userManager.RegisterTokenProvider("ResetPassword", emailUserTokenProvider);
            // _userManager.RegisterTokenProvider("SMS",new UserTokenProvider());
            // _userManager.RegisterTokenProvider("Phone", new UserTokenProvider());
            _emailSender = emailSender;
            _siteSettings = siteSettings.Value;
            _twilioSettings = twilioSettings.Value;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _localizer = localizer;
            _dbContext = dbContext;
        }


        [Authorize(Roles = Constants.AdminGroupName)]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = Constants.AdminGroupName)]
        [Route("Account/UserList/{page?}/{len?}")]
        public async Task<IActionResult> UserList(string page, string len)
        {
            int pageNum = page!=null ? int.Parse(page) : 0;
            int pageLen = len!=null ? int.Parse(len) : defaultLen;

            var users =  _dbContext.Users.OrderBy(u=>u.UserName);
            var shown = pageNum * pageLen;
            var toShow = users.Skip(shown).Take(pageLen);

            ViewBag.page = pageNum;
            ViewBag.hasNext = await users.CountAsync() > (toShow.Count() + shown);
            ViewBag.nextpage = pageNum+1;
            ViewBag.pageLen = pageLen;
            // ApplicationUser user;
            // user.EmailConfirmed
            return View(toShow.ToArray());
        }

        string GeneratePageToken() {
            return System.Guid.NewGuid().ToString();
        }
        
        [AllowAnonymous]
        [HttpGet(Constants.LoginPath)]
        public ActionResult SignIn(string returnUrl = null)
        {
            // Note: the "returnUrl" parameter corresponds to the endpoint the user agent
            // will be redirected to after a successful authentication and not
            // the redirect_uri of the requesting client application against the third
            // party identity provider.
            return View(new SignInViewModel
            {
                ReturnUrl = returnUrl ?? "/",
                ExternalProviders = HttpContext.GetExternalProviders()
            });
            /* 
            Note: When using an external login provider, redirect the query  :
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(OpenIdConnectDefaults.AuthenticationScheme, returnUrl);
            return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme, properties);
            */

        }

        [AllowAnonymous]
        public ActionResult AccessDenied(string requestUrl = null)
        {
            ViewBag.UserIsSignedIn = User.IsSignedIn();
            if (string.IsNullOrWhiteSpace(requestUrl))
                if (string.IsNullOrWhiteSpace(Request.Headers["Referer"]))
                    requestUrl = "/";
                else requestUrl = Request.Headers["Referer"];
            return View("AccessDenied", requestUrl);
        }

        [AllowAnonymous]
        [HttpPost(Constants.LoginPath)]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            if (Request.Method == "POST")
            {
                if (model.Provider ==null || model.Provider == "LOCAL")
                {
                    if (ModelState.IsValid)
                    {
                        var user = _dbContext.Users.Include(u=>u.Membership).FirstOrDefault(
                            u=>u.UserName == model.UserName);
                       
                        if (user != null)
                        {
                            if (!await _userManager.IsEmailConfirmedAsync(user))
                            {
                                ModelState.AddModelError(string.Empty, 
                                            "You must have a confirmed email to log in.");
                                return this.ViewOk(model);
                            }
                        }
                        else {
                                ModelState.AddModelError(string.Empty, 
                                            "No such user.");
                                return this.ViewOk(model);
                        }
                        // This doesn't count login failures towards account lockout
                        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                        var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                        if (result.Succeeded)
                        {
                            // FIXME should be done at granting resources
                            await _userManager.AddClaimsAsync(user, user.Membership.Select(
                                m => new Claim(YavscClaimTypes.CircleMembership, m.CircleId.ToString())
                             ));
                            return Redirect(model.ReturnUrl ?? "/");
                        }

                        if (result.RequiresTwoFactor)
                        {
                            return RedirectToAction(nameof(SendCode), new { returnUrl = model.ReturnUrl, rememberMe = model.RememberMe });
                        }
                        if (result.IsLockedOut)
                        {
                            _logger.LogWarning(2, "User account locked out.");
                            return this.ViewOk("Lockout");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, $"Invalid login attempt. ({model.UserName}, {model.Password})");
                            model.ExternalProviders = HttpContext.GetExternalProviders();
                            return this.ViewOk(model);
                        }
                    }

                    // If we got this far, something failed, redisplay form
                    ModelState.AddModelError(string.Empty, "Unexpected behavior: something failed ... you could try again, or contact me ...");
                }
                else
                {

                    // Note: the "provider" parameter corresponds to the external
                    // authentication provider choosen by the user agent.
                    if (string.IsNullOrEmpty(model.Provider))
                    {
                        _logger.LogWarning("Provider not specified");
                        return HttpBadRequest();
                    }

                    if (!_signInManager.GetExternalAuthenticationSchemes().Any(x => x.AuthenticationScheme == model.Provider))
                    {
                        _logger.LogWarning($"Provider not found : {model.Provider}");
                        return HttpBadRequest();
                    }

                    // Instruct the middleware corresponding to the requested external identity
                    // provider to redirect the user agent to its own authorization endpoint.
                    // Note: the authenticationScheme parameter must match the value configured in Startup.cs

                    // Note: the "returnUrl" parameter corresponds to the endpoint the user agent
                    // will be redirected to after a successful authentication and not
                    // the redirect_uri of the requesting client application.
                    if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        _logger.LogWarning("ReturnUrl not specified");
                        return HttpBadRequest();
                    }
                    // Note: this still is not the redirect uri given to the third party provider, at building the challenge.
                    var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { model.ReturnUrl }, protocol:"https", host: Startup.Authority);
                    var properties = _signInManager.ConfigureExternalAuthenticationProperties(model.Provider, redirectUrl);
                    // var properties = new AuthenticationProperties{RedirectUri=ReturnUrl};
                    return new ChallengeResult(model.Provider, properties);

                }
            }
            model.ExternalProviders = HttpContext.GetExternalProviders();
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation(3, "User created a new account with password.");
                    await _emailSender.SendEmailAsync(Startup.SiteSetup.Owner.Name, Startup.SiteSetup.Owner.EMail,
                     $"[{_siteSettings.Title}] Inscription avec mot de passe: {user.UserName} ", $"{user.Id}/{user.UserName}/{user.Email}");

                    // TODO user.DiskQuota = Startup.SiteSetup.UserFiles.Quota;
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol:  "https", host: Startup.Authority);
                    var emailSent = await _emailSender.SendEmailAsync(model.UserName, model.Email, _localizer["ConfirmYourAccountTitle"],
                      string.Format(_localizer["ConfirmYourAccountBody"], _siteSettings.Title, callbackUrl, _siteSettings.Slogan, _siteSettings.Audience));
                   // No, wait for more than a login pass submission:
                   // do not await _signInManager.SignInAsync(user, isPersistent: false);
                    if (emailSent==null)
                    {
                        _logger.LogWarning("User created with error sending email confirmation request");
                        this.NotifyWarning(
                                "E-mail confirmation",
                                _localizer["ErrorSendingEmailForConfirm"]
                         );
                    }
                    else   
                        this.NotifyInfo(
                             "E-mail confirmation",
                             _localizer["EmailSentForConfirm"]
                      );

                    return View("AccountCreated");
                }
                else {
                    _logger.LogError("Error registering from a valid model.");
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"{error.Code} {error.Description}");
                    }
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendConfirationEmail()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var model = await SendEMailForConfirmAsync(user);
            return View(model);
        }

        [Authorize("AdministratorOnly")]
        public async Task<IActionResult> AdminSendConfirationEmail(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var model = await SendEMailForConfirmAsync(user);
            return View(model);
        }

        private async Task<EmailSentViewModel> SendEMailForConfirmAsync(ApplicationUser user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account",
            new { userId = user.Id, code }, protocol: "https", host: Startup.Authority);
            var res = await _emailSender.SendEmailAsync(user.UserName, user.Email, 
            this._localizer["ConfirmYourAccountTitle"],
            string.Format(this._localizer["ConfirmYourAccountBody"],
                  _siteSettings.Title, callbackUrl, _siteSettings.Slogan,
                   _siteSettings.Audience));
            return res;
        }

        private async Task<EmailSentViewModel> SendEMailFactorAsync(ApplicationUser user, string provider)
        {
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, provider);
            var callbackUrl = Url.Action("VerifyCode", "Account",
            new { userId = user.Id, code, provider }, protocol: "https", host: Startup.Authority);
            var res = await _emailSender.SendEmailAsync(user.UserName, user.Email, 
            this._localizer["AccountEmailFactorTitle"],
            string.Format(this._localizer["AccountEmailFactorBody"],
                  _siteSettings.Title, callbackUrl, _siteSettings.Slogan,
                   _siteSettings.Audience, code));
            return res;
        }
        //
        // POST: /Account/LogOff
        [HttpPost(Constants.LogoutPath)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            _logger.LogInformation(4, "User logged out.");
            if (returnUrl == null) return RedirectToAction(nameof(HomeController.Index), "Home");
            return Redirect(returnUrl);
        }

        //
        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("No external provider info found.");
                return Redirect("~/signin"); // RedirectToAction(nameof(OAuthController.SignIn));
            }

            // Sign in the user with this external login provider if the user already has a login.
            info.ProviderDisplayName = info.ExternalPrincipal.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                _logger.LogInformation(5, $"User logged in with {info.LoginProvider} provider, as {info.ProviderDisplayName} ({info.ProviderKey}).");

                var ninfo = _dbContext.UserLogins.First(l => l.ProviderKey == info.ProviderKey && l.LoginProvider == info.LoginProvider);
                ninfo.ProviderDisplayName = info.ProviderDisplayName;
                _dbContext.Entry(ninfo).State = EntityState.Modified;
                _dbContext.SaveChanges(User.GetUserId());

                return Redirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe= true });
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                ViewData["jsonres"] = JsonConvert.SerializeObject(result);
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.ExternalPrincipal.FindFirstValue(ClaimTypes.Email);
                var name = info.ExternalPrincipal.FindFirstValue(ClaimTypes.Name);
                var avatar = info.ExternalPrincipal.FindFirstValue("urn:google:profile");
                /* var phone = info.ExternalPrincipal.FindFirstValue(ClaimTypes.HomePhone);
                 var mobile = info.ExternalPrincipal.FindFirstValue(ClaimTypes.MobilePhone);
                 var postalcode = info.ExternalPrincipal.FindFirstValue(ClaimTypes.PostalCode);
                 var locality = info.ExternalPrincipal.FindFirstValue(ClaimTypes.Locality);
                 var country = info.ExternalPrincipal.FindFirstValue(ClaimTypes.Country);
                foreach (var claim in info.ExternalPrincipal.Claims)
                    _logger.LogWarning("# {0} Claim: {1} {2}", info.LoginProvider, claim.Type, claim.Value);
*/
                var access_token = info.ExternalPrincipal.FindFirstValue("access_token");
                var token_type = info.ExternalPrincipal.FindFirstValue("token_type");
                var expires_in = info.ExternalPrincipal.FindFirstValue("expires_in");

                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel
                {
                    Email = email,
                    Name = name
                });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            if (User.IsSignedIn())
            {
                return RedirectToAction(nameof(ManageController.Index), "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Name, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    info.ProviderDisplayName = info.ExternalPrincipal.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);


                        await _emailSender.SendEmailAsync(Startup.SiteSetup.Owner.Name, Startup.SiteSetup.Owner.EMail,
                         $"[{_siteSettings.Title}] Inscription via {info.LoginProvider}: {user.UserName} ", $"{user.Id}/{user.UserName}/{user.Email}");

                        _logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);

                        return Redirect(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            IdentityResult result=null;
            try { 
                result = await _userManager.ConfirmEmailAsync(user, code);
                _dbContext.SaveChanges(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
            }
            return View(result.Succeeded ? "EmailConfirmed" : "Error");
        }

        // GET: /Account/ConfirmTwoFactorToken
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmTwoFactorToken(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            bool result=false;
            try { 
                result = await _userManager.VerifyTwoFactorTokenAsync(user, Constants.DefaultFactor, code);
                _dbContext.SaveChanges(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
            }
            return View(result ? "EmailConfirmed" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            if (User.Identity.IsAuthenticated)
              ViewBag.UserEmail = ( await _dbContext.Users.SingleAsync(
                  u => u.Id == User.GetUserId()
              ) ).Email;
              
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user;
                // Username should not contain any '@'
                if (model.LoginOrEmail.Contains('@')) {
                    user = await _userManager.FindByEmailAsync(model.LoginOrEmail);
                }
                else {
                    user = await _dbContext.Users.FirstOrDefaultAsync( u => u.UserName == model.LoginOrEmail);
                }

                // Don't reveal that the user does not exist or is not confirmed
                if (user == null)
                {
                    _logger.LogWarning($"ForgotPassword: Email or User name {model.LoginOrEmail} not found");
                    return View("ForgotPasswordConfirmation");
                }
                // We cannot require the email to be confimed,
                // or a lot of non confirmed email never be able to finalyze
                // registration.
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    _logger.LogWarning($"ForgotPassword: Email {model.LoginOrEmail} not confirmed");
                    // don't break this recovery process here ...
                    // or else e-mail won't ever be validated, since user lost his password.
                    // don't return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code },
                protocol: "https", host: Startup.Authority);
                var sent = await _emailSender.SendEmailAsync(user.UserName, user.Email, _localizer["Reset Password"],
                    _localizer["Please reset your password by following this link:"] + " <" + callbackUrl + ">");
                return View("ForgotPasswordConfirmation", sent);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user==null) return new BadRequestResult();
            // We just serve the form to reset here.
            return View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (result.Succeeded)
            {
                // when ok, the e-mail become validated.
                if (!user.EmailConfirmed) {
                    user.EmailConfirmed=true;
                    await _dbContext.SaveChangesAsync(nameof(ResetPassword));
                }
            _logger.LogInformation($"Password reset for {user.UserName}:{model.Password}");
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            _logger.LogInformation($"Password reset failed for {user.UserName}:{model.Password}");
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/SendCode
        [HttpGet, AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = true)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error", new Exception("No Two factor authentication user"));
            }

            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);

            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [ValidateAntiForgeryToken, AllowAnonymous]
        public async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error", new Exception("user is null"));
            }

            // Generate the token and send it
            if (model.SelectedProvider == Constants.MobileAppFactor)
            {
                return View("Error", new Exception("No mobile app service was activated"));
            }
            else 
            if (model.SelectedProvider == Constants.SMSFactor)
            {
                return View("Error", new Exception("No SMS service was activated"));
                // await _smsSender.SendSmsAsync(_twilioSettings, await _userManager.GetPhoneNumberAsync(user), message);
            }
            else // if (model.SelectedProvider == Constants.EMailFactor || model.SelectedProvider == "Default" )
            {
               var sent = await this.SendEMailFactorAsync(user, model.SelectedProvider);
            }
            return View("VerifyCode", new VerifyCodeViewModel { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/VerifyCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string code, string provider, bool rememberMe=true, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error", new Exception("user is null"));
            }
            // it may be a GET response from some email url, or the web response to second factor requirement
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe, Code = code });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            _logger.LogWarning("Signin with code: {0} {1}", model.Provider, model.Code);

            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberMe);
            if (result.Succeeded)
            {
                ViewData["StatusMessage"] = "Your code was verified";
                _logger.LogInformation($"Signed in. returning to {model.ReturnUrl}");
                if (model.ReturnUrl!=null)
                return Redirect(model.ReturnUrl);
                else RedirectToAction("Index","Home");
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError("Code", "Code invalide ");
                return View(model);
            }
        }

        [HttpGet, Authorize]
        public IActionResult Delete()
        {
            return View(new UnregisterViewModel { UserId = User.GetUserId() });
        }

        [HttpGet, Authorize("AdministratorOnly")]
        public IActionResult AdminDelete(string id)
        {
            return View(new UnregisterViewModel { UserId = id });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Delete(UnregisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await DeleteUser(model.UserId);
            
            if (!result.Succeeded)
            {
                AddErrors(result);
                return new BadRequestObjectResult(ModelState);
            }
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
        async Task<IdentityResult> DeleteUser(string userId)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);

            _dbContext.DeviceDeclaration.RemoveRange( _dbContext.DeviceDeclaration.Where(g => g.DeviceOwnerId == userId ));
            
            foreach (var template in _dbContext.MailingTemplate.Where( t=>t.ManagerId == userId )) 
            {
                template.ManagerId = template.SuccessorId;
            }

            return await _userManager.DeleteAsync(user);
        }

        [HttpPost, Authorize("AdministratorOnly")]
        public async Task<IActionResult> AdminDelete(UnregisterViewModel model)
        {
            var result = await DeleteUser(model.UserId);

            if (!result.Succeeded)
            {
                AddErrors(result);
                return new BadRequestObjectResult(ModelState);
            }
            return View();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, _localizer[error.Code]);
            }
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await GetCurrentUserAsync(HttpContext.User.GetUserId());
        }
        private async Task<ApplicationUser> GetCurrentUserAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        #endregion
    }
}
