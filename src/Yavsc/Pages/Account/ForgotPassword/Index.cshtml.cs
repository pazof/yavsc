
using System.Web;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Yavsc.Helpers;
using Yavsc.Interface;
using Yavsc.Models;
using Yavsc.ViewModels.Account;

namespace Yavsc.Pages.ForgotPassword;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IIdentityProviderStore _identityProviderStore;
    private readonly IEventService _events;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Index> _logger;
    private readonly SiteSettings _siteSettings;
    private readonly ITrueEmailSender _emailSender;
    private readonly IStringLocalizer<YavscLocalization> _localizer;

    [BindProperty]
    public ForgotPasswordViewModel Input { get; set; }

    public Index(
        IIdentityServerInteractionService interaction,
        IAuthenticationSchemeProvider schemeProvider,
        IIdentityProviderStore identityProviderStore,
        IEventService events,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext applicationDbContext,
        ILoggerFactory loggerFactory,
        ITrueEmailSender emailSender,
        IStringLocalizer<Yavsc.YavscLocalization> localizer,
        IOptions<SiteSettings> siteSettings
        )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _schemeProvider = schemeProvider;
        _identityProviderStore = identityProviderStore;
        _events = events;
        _dbContext = applicationDbContext;
        _logger = loggerFactory.CreateLogger<Index>();
        _siteSettings = siteSettings.Value;
        _emailSender = emailSender;
        _localizer = localizer;
    }
    
    public async Task<IActionResult> OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        ApplicationUser user;
        // Username should not contain any '@'
        if (Input.LoginOrEmail.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(Input.LoginOrEmail);
        }
        else
        {
            user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == Input.LoginOrEmail);
        }

        // Don't reveal that the user does not exist or is not confirmed
        if (user == null)
        {
            _logger.LogWarning($"ForgotPassword: Email or User name {Input.LoginOrEmail} not found");
            return Redirect("ForgotPasswordConfirmation");
        }
        // We cannot require the email to be confimed,
        // or a lot of non confirmed email never be able to finalyze
        // registration.
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            _logger.LogWarning($"ForgotPassword: Email {Input.LoginOrEmail} not confirmed");
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
        return Page();
    }
}
