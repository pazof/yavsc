
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
    using PayPal.PayPalAPIInterfaceService;
    using PayPal.PayPalAPIInterfaceService.Model;

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
            
            await usrActs.Where(u=>u.Does.SettingsClassName!=null).ForEachAsync(
                a=>{ a.Settings = a.Does.CreateSettings(User.GetUserId()); }
            );

            model.HaveActivityToConfigure = usrActs.Any( a=> a.Settings == null && a.Does.SettingsClassName!=null );
            model.Activity = _dbContext.UserActivities.Include(a=>a.Does).Where(u=>u.UserId == user.Id)
            .ToList();
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
            await _dbContext.SaveChangesAsync(User.GetUserId());
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
            return View();
        }

        [HttpGet, Authorize]
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
        public  Dictionary<string, string> PaypalConfig { 
            get { 
                var config = 
                new Dictionary<string, string>();
            config.Add("mode", "sandbox");
            config.Add("account1.apiUsername", _payPalSettings.UserId);
            config.Add("account1.apiPassword", _payPalSettings.Secret);
            config.Add("account1.apiSignature", _payPalSettings.Signature);
            return config;
            }
        } 


        protected IActionResult DoDirectCredit(DoDirectCreditViewModel model)
        {
            // Create request object
            DoDirectPaymentRequestType request = new DoDirectPaymentRequestType();
            DoDirectPaymentRequestDetailsType requestDetails = new DoDirectPaymentRequestDetailsType();
            request.DoDirectPaymentRequestDetails = requestDetails;

            // (Optional) How you want to obtain payment. It is one of the following values:
            // * Authorization – This payment is a basic authorization subject to settlement with PayPal Authorization and Capture.
            // * Sale – This is a final sale for which you are requesting payment (default).
            // Note: Order is not allowed for Direct Payment.
            requestDetails.PaymentAction = (PaymentActionCodeType)
                Enum.Parse(typeof(PaymentActionCodeType), model.PaymentType);

            // (Required) Information about the credit card to be charged.
            CreditCardDetailsType creditCard = new CreditCardDetailsType();
            requestDetails.CreditCard = creditCard;
            PayerInfoType payer = new PayerInfoType();
            // (Optional) First and last name of buyer.
            PersonNameType name = new PersonNameType();
            name.FirstName = model.FirstName;
            name.LastName = model.LastName;
            payer.PayerName = name;
            // (Required) Details about the owner of the credit card.
            creditCard.CardOwner = payer;

            // (Required) Credit card number.
            creditCard.CreditCardNumber = model.CreditCardNumber;
            // (Optional) Type of credit card. For UK, only Maestro, MasterCard, Discover, and Visa are allowable. For Canada, only MasterCard and Visa are allowable and Interac debit cards are not supported. It is one of the following values:
            // * Visa
            // * MasterCard
            // * Discover
            // * Amex
            // * Maestro: See note.
            // Note: If the credit card type is Maestro, you must set currencyId to GBP. In addition, you must specify either StartMonth and StartYear or IssueNumber.
            creditCard.CreditCardType = (CreditCardTypeType)
                Enum.Parse(typeof(CreditCardTypeType), model.CreditCardType);
            // Card Verification Value, version 2. Your Merchant Account settings determine whether this field is required. To comply with credit card processing regulations, you must not store this value after a transaction has been completed.
            // Character length and limitations: For Visa, MasterCard, and Discover, the value is exactly 3 digits. For American Express, the value is exactly 4 digits.
            creditCard.CVV2 = model.Cvv2Number;
            string[] cardExpiryDetails = model.CardExpiryDate.Split(new char[] { '/' });
            if (cardExpiryDetails.Length == 2)
            {
                // (Required) Credit card expiration month.
                creditCard.ExpMonth = Convert.ToInt32(cardExpiryDetails[0]);
                // (Required) Credit card expiration year.
                creditCard.ExpYear = Convert.ToInt32(cardExpiryDetails[1]);
            }

            requestDetails.PaymentDetails = new PaymentDetailsType();
            // (Optional) Your URL for receiving Instant Payment Notification (IPN) about this transaction. If you do not specify this value in the request, the notification URL from your Merchant Profile is used, if one exists.
            // Important: The notify URL applies only to DoExpressCheckoutPayment. This value is ignored when set in SetExpressCheckout or GetExpressCheckoutDetails.
            requestDetails.PaymentDetails.NotifyURL = model.IpnNotificationUrl.Trim();

            // (Optional) Buyer's shipping address information. 
            AddressType billingAddr = new AddressType();
            if (model.FirstName != string.Empty && model.LastName != string.Empty
                && model.Street1 != string.Empty && model.Country != string.Empty)
            {
                billingAddr.Name = model.PayerName;
                // (Required) First street address.
                billingAddr.Street1 = model.Street1;
                // (Optional) Second street address.
                billingAddr.Street2 = model.Street2;
                // (Required) Name of city.
                billingAddr.CityName = model.City;
                // (Required) State or province.
                billingAddr.StateOrProvince = model.State;
                // (Required) Country code.
                billingAddr.Country = (CountryCodeType)Enum.Parse(typeof(CountryCodeType), model.Country);
                // (Required) U.S. ZIP code or other country-specific postal code.
                billingAddr.PostalCode = model.PostalCode;

                // (Optional) Phone number.
                billingAddr.Phone = model.Phone;

                payer.Address = billingAddr;
            }

            // (Required) The total cost of the transaction to the buyer. If shipping cost and tax charges are known, include them in this value. If not, this value should be the current subtotal of the order. If the transaction includes one or more one-time purchases, this field must be equal to the sum of the purchases. This field must be set to a value greater than 0.
            // Note: You must set the currencyID attribute to one of the 3-character currency codes for any of the supported PayPal currencies.
            CurrencyCodeType currency = (CurrencyCodeType)
                Enum.Parse(typeof(CurrencyCodeType), model.CurrencyCode);
            BasicAmountType paymentAmount = new BasicAmountType(currency, model.Amount);            
            requestDetails.PaymentDetails.OrderTotal = paymentAmount;

            // Invoke the API
            DoDirectPaymentReq wrapper = new DoDirectPaymentReq();
            wrapper.DoDirectPaymentRequest = request;

            // Configuration map containing signature credentials and other required configuration.
            // For a full list of configuration parameters refer in wiki page 
            // [https://github.com/paypal/sdk-core-dotnet/wiki/SDK-Configuration-Parameters]
            //// TODO clean Dictionary<string, string> configurationMap = Configuration.GetAcctAndConfig();

            // Create the PayPalAPIInterfaceServiceService service object to make the API call
            PayPalAPIInterfaceServiceService service = new PayPalAPIInterfaceServiceService(PaypalConfig);

            // # API call 
            // Invoke the DoDirectPayment method in service wrapper object  
            DoDirectPaymentResponseType response = service.DoDirectPayment(wrapper);
         
            // Check for API return status
            return setKeyResponseObjects(service, response);
        }

        private IActionResult setKeyResponseObjects(PayPalAPIInterfaceServiceService service, DoDirectPaymentResponseType response)
        {
            HttpContext.Items.Add("Response_apiName", "DoDirectPayment");
            HttpContext.Items.Add("Response_redirectURL", null);
            HttpContext.Items.Add("Response_requestPayload", service.getLastRequest());
            HttpContext.Items.Add("Response_responsePayload", service.getLastResponse());

            Dictionary<string, string> responseParams = new Dictionary<string, string>();
            responseParams.Add("Correlation Id", response.CorrelationID);
            responseParams.Add("API Result", response.Ack.ToString());

            if (response.Ack.Equals(AckCodeType.FAILURE) ||
                (response.Errors != null && response.Errors.Count > 0))
            {
                HttpContext.Items.Add("Response_error", response.Errors);
            }
            else
            {
                HttpContext.Items.Add("Response_error", null);                
                responseParams.Add("Transaction Id", response.TransactionID);
                responseParams.Add("Payment status", response.PaymentStatus.ToString());
                if(response.PendingReason != null) {
                    responseParams.Add("Pending reason", response.PendingReason.ToString());
                }
            }
            HttpContext.Items.Add("Response_keyResponseObject", responseParams);
            return View("APIResponse");
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
