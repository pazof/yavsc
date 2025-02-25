using System.Web;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Yavsc.Models;
using Yavsc.ViewModels.Account;
using Yavsc.Helpers;
using Yavsc.Abstract.Manage;
using Yavsc.Interface;
using IdentityServer8.Test;
using IdentityServer8.Services;
using IdentityServer8.Stores;
using Microsoft.AspNetCore.Authentication;
using Yavsc.Models.Access;
using IdentityServer8.Models;
using Yavsc.Extensions;
using IdentityServer8.Events;
using IdentityServer8.Extensions;
using IdentityServer8;
using IdentityModel;
using System.Security.Cryptography;
using System.Text.Unicode;
using System.Text;

namespace Yavsc.Controllers
{

    public class AccountController : Controller
    {
        private const string nextPageTokenKey = "nextPageTokenKey";
        private const int defaultLen = 10;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITrueEmailSender _emailSender;
        // private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        readonly SiteSettings _siteSettings;
        readonly TwilioSettings _twilioSettings;

        readonly IStringLocalizer _localizer;

        //  TwilioSettings _twilioSettings;

        readonly ApplicationDbContext _dbContext;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            RoleManager<IdentityRole> roleManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITrueEmailSender emailSender,
            IOptions<SiteSettings> siteSettings,
            ILoggerFactory loggerFactory, IOptions<TwilioSettings> twilioSettings,
            IStringLocalizer<Yavsc.YavscLocalization> localizer,
            ApplicationDbContext dbContext)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _roleManager = roleManager;

            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _siteSettings = siteSettings.Value;
            _twilioSettings = twilioSettings.Value;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _localizer = localizer;
            _dbContext = dbContext;
        }


        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {

            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("Redirect", model.ReturnUrl);
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
              
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user!=null) {

               
                   var signin = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);
     
                    // validate username/password against in-memory store
                    if (signin.Succeeded)
                    {
                        await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

                        // only set explicit expiration here if user chooses "remember me". 
                        // otherwise we rely upon expiration configured in cookie middleware.
                        await HttpContext.SignInAsync(user, _roleManager, model.RememberLogin,_dbContext);

                        if (context != null)
                        {
                            if (context.IsNativeClient())
                            {
                                // The client is native, so this change in how to
                                // return the response is for better UX for the end user.
                                return this.LoadingPage("Redirect", model.ReturnUrl);
                            }

                            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                            return Redirect(model.ReturnUrl);
                        }

                        // request for a local page
                        if (Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }
                        else if (string.IsNullOrEmpty(model.ReturnUrl))
                        {
                            return Redirect("~/");
                        }
                        else
                        {
                            // user might have clicked on a malicious link - should be logged
                            throw new Exception("invalid return URL");
                        }
                    }
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId:context?.Client.ClientId));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet][Authorize]
        public async Task<IActionResult> Logout(string logoutId)
        {
            if (string.IsNullOrWhiteSpace(logoutId))
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        logoutId = User.GetUserId();
                    }
                }
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));

            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

        


            return View("LoggedOut", vm);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer8.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer8.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }


        [Authorize(Roles = Constants.AdminGroupName)]
        public IActionResult Index()
        {
            IViewComponentHelper h;
           
            return View();
        }

        [Authorize(Roles = Constants.AdminGroupName)]
        [Route("Account/UserList/{pageNum}/{len?}")]
        public async Task<IActionResult> UserList(int pageNum, int pageLen = defaultLen)
        {
            var users =  _dbContext.Users.OrderBy(u=>u.UserName);
            var shown = pageNum * pageLen;
            var toShow = users.Skip(shown).Take(pageLen);

            ViewBag.page = pageNum;
            ViewBag.hasNext =  users.Count() > (toShow.Count() + shown);
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
            return View(new SignInModel
            {
                ReturnUrl = returnUrl ?? "/",
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
            ViewBag.UserIsSignedIn = User.Identity.IsAuthenticated;
            
            if (string.IsNullOrWhiteSpace(requestUrl))
                if (string.IsNullOrWhiteSpace(Request.Headers["Referer"]))
                    requestUrl = "/";
                else requestUrl = Request.Headers["Referer"];
            return View("AccessDenied", requestUrl);
        }

        [AllowAnonymous]
        [HttpPost(Constants.LoginPath)]
        public async Task<IActionResult> SignIn(SignInModel model)
        {
            if (Request.Method == "POST") // "hGbkk9B94NAae#aG"

            {
                if (model.Provider ==null || model.Provider == "LOCAL")
                {
                    if (ModelState.IsValid)
                    {
                        var user = _dbContext.Users.Include(u=>u.Membership).FirstOrDefault(
                            u=>u.Email == model.EMail);
                       
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
                        var result = await _signInManager.PasswordSignInAsync(model.EMail, model.Password, model.RememberMe, lockoutOnFailure: false);

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
                            ModelState.AddModelError(string.Empty, $"Invalid login attempt. ({model.EMail}, {model.Password})");
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
                        return BadRequest();
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
                        return BadRequest();
                    }
                    // Note: this still is not the redirect uri given to the third party provider, at building the challenge.
                    var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { model.ReturnUrl }, protocol:"https", host: Config.Authority);
                    var properties = _signInManager.ConfigureExternalAuthenticationProperties(model.Provider, redirectUrl);
                    // var properties = new AuthenticationProperties{RedirectUri=ReturnUrl};
                    return new ChallengeResult(model.Provider, properties);

                }
            }
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
                    await _emailSender.SendEmailAsync(Config.SiteSetup.Owner.Name, Config.SiteSetup.Owner.EMail,
                     $"[{_siteSettings.Title}] Inscription avec mot de passe: {user.UserName} ", $"{user.Id}/{user.UserName}/{user.Email}");

                    // TODO user.DiskQuota = Startup.SiteSetup.UserFiles.Quota;
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol:  "https", host: Config.Authority);
                    await _emailSender.SendEmailAsync(model.UserName, model.Email, _localizer["ConfirmYourAccountTitle"],
                      string.Format(_localizer["ConfirmYourAccountBody"], _siteSettings.Title, callbackUrl, _siteSettings.Slogan, _siteSettings.Audience));
                   // No, wait for more than a login pass submission:
                   // do not await _signInManager.SignInAsync(user, isPersistent: false);
                    
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
            new { userId = user.Id, code }, protocol: "https", host: Config.Authority);
            var res = await _emailSender.SendEmailAsync(user.UserName, user.Email, 
            this._localizer["ConfirmYourAccountTitle"],
            string.Format(this._localizer["ConfirmYourAccountBody"],
                  _siteSettings.Title, callbackUrl, _siteSettings.Slogan,
                   _siteSettings.Audience));
            return new EmailSentViewModel { EMail = user.Email, Sent = true, MessageId = res };
        }

        private async Task<EmailSentViewModel> SendEMailFactorAsync(ApplicationUser user, string provider)
        {
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, provider);
            var callbackUrl = Url.Action("VerifyCode", "Account",
            new { userId = user.Id, code, provider }, protocol: "https", host: Config.Authority);
            var res = await _emailSender.SendEmailAsync(user.UserName, user.Email, 
            this._localizer["AccountEmailFactorTitle"],
            string.Format(this._localizer["AccountEmailFactorBody"],
                  _siteSettings.Title, callbackUrl, _siteSettings.Slogan,
                   _siteSettings.Audience, code));
            return new EmailSentViewModel { EMail = user.Email, Sent = true, MessageId = res };;
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
            throw new NotImplementedException();
            // info.ProviderDisplayName = info.ExternalPrincipal.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

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
                var email = info.AuthenticationProperties.GetParameter<string>(ClaimTypes.Email);
                var name = info.AuthenticationProperties.GetParameter<string>(ClaimTypes.Name);
                var avatar = info.AuthenticationProperties.GetParameter<string>("urn:google:profile");
                /* var phone = info.ExternalPrincipal.FindFirstValue(ClaimTypes.HomePhone);
                 var mobile = info.ExternalPrincipal.FindFirstValue(ClaimTypes.MobilePhone);
                 var postalcode = info.ExternalPrincipal.FindFirstValue(ClaimTypes.PostalCode);
                 var locality = info.ExternalPrincipal.FindFirstValue(ClaimTypes.Locality);
                 var country = info.ExternalPrincipal.FindFirstValue(ClaimTypes.Country);
                foreach (var claim in info.ExternalPrincipal.Claims)
                    _logger.LogWarning("# {0} Claim: {1} {2}", info.LoginProvider, claim.Type, claim.Value);
*/
                var access_token = info.AuthenticationProperties.GetParameter<string>("access_token");
                var token_type = info.AuthenticationProperties.GetParameter<string>("token_type");
                var expires_in = info.AuthenticationProperties.GetParameter<string>("expires_in");

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
                    
                    info.ProviderDisplayName = info.Principal.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);


                        await _emailSender.SendEmailAsync(Config.SiteSetup.Owner.Name, Config.SiteSetup.Owner.EMail,
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
                var callbackUrl = _siteSettings.Audience + "/Account/ResetPassword/" + 
                    HttpUtility.UrlEncode(user.Id) + "/" + HttpUtility.UrlEncode(code);
                
                var sent = await _emailSender.SendEmailAsync(user.UserName, user.Email, _localizer["Reset Password"],
                    _localizer["Please reset your password by "] + " <a href=\"" +
                    callbackUrl + "\" >following this link</a>");
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
        [HttpGet("/Account/ResetPassword/{id}/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string id, string code)
        {
            var user = await _userManager.FindByIdAsync(id);
            
            if (user==null) return new BadRequestResult();
            // We just serve the form to reset here.
            return View(new ResetPasswordViewModel { 
                Id = id,
                Code = code,
                Email = user.Email
            });
        }

        // POST: /Account/ResetPassword
        [HttpPost("/Account/ResetPassword/{id}/{code}")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromRoute] string id, 
        [FromRoute] string code, 
         ResetPasswordViewModel model)
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
        // code : "CfDJ8DmPlC3R8%2fNMqGlHZHZMwbjaXxgD3GW3H75Ubt+4Sbw%2fn%2fdg9X8Bll+CLIh%2fquI+Z96XEkx7bfrZiB+wpPb+b5%2ffgzgy+cQnKfX9J7%2fLNro+F3uE5JkXSlUc1WqVW2mVQrpWHjx1Dbn2n77TTGym3ttQoECsTR%2foo27dW9U11pmRJuTiwPBJZBOt0ffIRmgDDHh2f0VySTQEwjfRiLdCwctL%2fmh21ympJMKJl5PZnTVs"
            var result = await _userManager.ResetPasswordAsync(user,
            HttpUtility.UrlDecode(code), model.Password);

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
