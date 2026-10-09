#nullable enable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImageMagick;

using Yavsc.Models;
using Yavsc.Api.Helpers;
using Yavsc.Server.Helpers;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Workflow;
using Yavsc.Interface;
using Yavsc.Services;

namespace Yavsc.WebApi.Controllers
{
    [Route( Constants.APIPrefix + "/account")]
    [Authorize("ApiScope")]
    public class ApiAccountController : Controller
    {
        private const long MaxAvatarSizeBytes = 2 * 1024 * 1024;
        private static readonly string[] AcceptedAvatarMimeTypes =
        [
            "image/png",
            "image/jpeg",
            "image/webp",
            "image/gif",
            "image/bmp",
            "image/tiff",
        ];

        readonly ApplicationDbContext _dbContext;
        private readonly SiteSettings siteSettings;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITrueEmailSender _emailSender;
        private readonly ICalendarManager _calendarManager;

        public ApiAccountController(
        ILoggerFactory loggerFactory, ApplicationDbContext dbContext,
        IOptions<SiteSettings> siteSettings, UserManager<ApplicationUser> userManager,
        ITrueEmailSender emailSender, ICalendarManager calendarManager)
        {
            _logger = loggerFactory.CreateLogger(nameof(ApiAccountController));
            _dbContext = dbContext;
            this.siteSettings = siteSettings.Value;
            _userManager = userManager;
            _emailSender = emailSender;
            _calendarManager = calendarManager;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Challenge();
            }

            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            var userData = await _dbContext.Users
                .Include(u => u.PostalAddress)
                .Include(u => u.AccountBalance)
                .Include(u => u.BankInfo)
                .FirstOrDefaultAsync(u => u.Id == uid);

            if (userData is null)
            {
                return NotFound(new { error = "user not found" });
            }

            var user = new Yavsc.Abstract.Identity.Me
            {
                UserId = userData.Id,
                UserName = userData.UserName,
                Email = userData.Email,
                Avatar = userData.Avatar,
                FullName = userData.FullName,
                Address = userData.PostalAddress?.Address ?? string.Empty,
                DedicatedGoogleCalendar = userData.DedicatedGoogleCalendar
            };
            user.BankInfoSummary = userData.BankInfo is { Count: > 0 }
                ? string.Join(" | ", userData.BankInfo.Select(b => string.IsNullOrWhiteSpace(b.IBAN) ? b.BIC : $"{b.IBAN} / {b.BIC}"))
                : string.Empty;

            var userRoles = await _dbContext.UserRoles
                .Where(u => u.UserId == uid)
                .Select(r => r.RoleId)
                .ToArrayAsync();

            var roles = await _dbContext.Roles
                .Where(r => userRoles.Contains(r.Id))
                .ToArrayAsync();

            user.Roles = roles.Select(r => r.Name).OfType<string>().ToArray();
            user.EmailConfirmed = await _userManager.IsEmailConfirmedAsync(userData);
            user.AllowMonthlyEmail = userData.AllowMonthlyEmail;
            user.HasPassword = await _userManager.HasPasswordAsync(userData);
            user.TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(userData);
            var externalLogins = await _userManager.GetLoginsAsync(userData);
            user.ExternalLoginCount = externalLogins.Count;
            user.ExternalLogins = externalLogins.Select(login => new LinkedExternalLogin
            {
                Provider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                DisplayName = login.ProviderDisplayName ?? login.LoginProvider
            }).ToArray();
            user.PostsCounter = await _dbContext.BlogSpot.LongCountAsync(x => x.AuthorId == uid);
            user.DiskUsage = userData.DiskUsage;
            user.DiskQuota = userData.DiskQuota;
            user.Credits = userData.AccountBalance?.Credits ?? 0;
            user.BankAccounts = (userData.BankInfo ?? [])
                .Select(bank => new BankAccountInfo
                {
                    Id = bank.Id,
                    IBAN = bank.IBAN,
                    BIC = bank.BIC,
                    BankCode = bank.BankCode,
                    WicketCode = bank.WicketCode,
                    AccountNumber = bank.AccountNumber,
                    BankedKey = bank.BankedKey
                })
                .ToArray();

            return Ok(user);
        }

        private async Task<ApplicationUser> GetUserData(string uid)
        {
            return await _dbContext.Users
                            .Include(u => u.PostalAddress)
                            .Include(u => u.AccountBalance)
                            .Include(u => u.BankInfo)
                            .FirstAsync(u => u.Id == uid);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] ProfileUpdateRequest request)
        {
            if (request is null)
            {
                return BadRequest(new { error = "Payload is required." });
            }

            if (request.BankInfoSummary is not null)
            {
                return BadRequest(new { error = "Bank information must be managed through the bank-info endpoint." });
            }

            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            var user = await _dbContext.Users
                .Include(u => u.PostalAddress)
                .Include(u => u.BankInfo)
                .FirstOrDefaultAsync(u => u.Id == uid);

            if (user is null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                var trimmedUserName = request.UserName.Trim();
                if (!string.Equals(user.UserName, trimmedUserName, StringComparison.Ordinal))
                {
                    var usernameUpdate = await _userManager.SetUserNameAsync(user, trimmedUserName);
                    if (!usernameUpdate.Succeeded)
                    {
                        return BadRequest(new { error = "Unable to update username.", details = usernameUpdate.Errors.Select(e => e.Description) });
                    }
                }
            }

            if (request.FullName is not null)
            {
                user.FullName = request.FullName.Trim();
            }

            if (request.Address is not null)
            {
                var trimmedAddress = request.Address.Trim();
                if (string.IsNullOrWhiteSpace(trimmedAddress))
                {
                    user.PostalAddress = null;
                }
                else if (user.PostalAddress is null)
                {
                    user.PostalAddress = new Yavsc.Models.Relationship.Location { Address = trimmedAddress };
                }
                else
                {
                    user.PostalAddress.Address = trimmedAddress;
                }
            }

            if (request.GoogleCalendarId is not null)
            {
                user.DedicatedGoogleCalendar = request.GoogleCalendarId.Trim();
            }

            await _dbContext.SaveChangesAsync(uid);
            return Ok(new { status = "saved", fullName = user.FullName, address = user.PostalAddress?.Address, googleCalendarId = user.DedicatedGoogleCalendar });
        }

        [HttpPut("preferences/monthly-email")]
        public async Task<IActionResult> UpdateMonthlyEmailPreference([FromBody] MonthlyEmailPreferenceRequest request)
        {
            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
            {
                return NotFound();
            }

            user.AllowMonthlyEmail = request.Enabled;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { error = "Unable to update email preferences.", details = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { enabled = user.AllowMonthlyEmail });
        }

        [HttpPut("security/two-factor")]
        public async Task<IActionResult> UpdateTwoFactor([FromBody] TwoFactorRequest request)
        {
            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
            {
                return NotFound();
            }

            var result = await _userManager.SetTwoFactorEnabledAsync(user, request.Enabled);
            if (!result.Succeeded)
            {
                return BadRequest(new { error = "Unable to update two-factor authentication.", details = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { enabled = user.TwoFactorEnabled });
        }

        [HttpPut("security/password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request is null
                || string.IsNullOrWhiteSpace(request.NewPassword)
                || !string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
            {
                return BadRequest(new { error = "A matching new password and confirmation are required." });
            }

            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
            {
                return NotFound();
            }

            var result = await _userManager.HasPasswordAsync(user)
                ? await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword)
                : await _userManager.AddPasswordAsync(user, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { error = "Unable to change the password.", details = result.Errors.Select(e => e.Description) });
            }

            return NoContent();
        }

        [HttpDelete("security/external-logins")]
        public async Task<IActionResult> RemoveExternalLogin([FromQuery] string provider, [FromQuery] string providerKey)
        {
            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(providerKey))
            {
                return BadRequest(new { error = "A provider and provider key are required." });
            }

            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
            {
                return NotFound();
            }

            if (await _userManager.GetLoginsAsync(user) is { Count: <= 1 }
                && !await _userManager.HasPasswordAsync(user))
            {
                return Conflict(new { error = "Add a password before removing the only sign-in method." });
            }

            var result = await _userManager.RemoveLoginAsync(user, provider, providerKey);
            if (!result.Succeeded)
            {
                return BadRequest(new { error = "Unable to remove the external login.", details = result.Errors.Select(e => e.Description) });
            }

            return NoContent();
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMyAccount([FromBody] DeleteAccountRequest request)
        {
            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
            {
                return NotFound();
            }

            if (!string.Equals(request?.UsernameConfirmation, user.UserName, StringComparison.Ordinal))
            {
                return BadRequest(new { error = "Type your exact username to confirm account deletion." });
            }

            _dbContext.DeviceDeclaration.RemoveRange(
                _dbContext.DeviceDeclaration.Where(device => device.DeviceOwnerId == uid));
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { error = "Unable to delete this account.", details = result.Errors.Select(e => e.Description) });
            }

            return NoContent();
        }

        [HttpPost("email/confirmation")]
        public async Task<IActionResult> SendEmailConfirmation()
        {
            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
            {
                return NotFound();
            }

            if (user.EmailConfirmed)
            {
                return Conflict(new { error = "This email address is already confirmed." });
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest(new { error = "No email address is associated with this account." });
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationUrl = new Uri(
                new Uri(siteSettings.ExternalUrl.TrimEnd('/') + "/", UriKind.Absolute),
                "Account/ConfirmEmail").ToString();
            confirmationUrl = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
                confirmationUrl,
                new Dictionary<string, string?> { ["userId"] = user.Id, ["code"] = code });

            await _emailSender.SendEmailAsync(
                user.UserName ?? user.Email,
                user.Email,
                $"[{siteSettings.Title}] Confirmation de votre adresse e-mail",
                $"<p>Confirmez votre adresse e-mail en suivant ce lien :</p><p><a href=\"{System.Text.Encodings.Web.HtmlEncoder.Default.Encode(confirmationUrl)}\">Confirmer mon adresse</a></p>");

            return Ok(new { message = "Un e-mail de confirmation a été envoyé." });
        }

        [HttpGet("calendars")]
        public async Task<IActionResult> GetCalendars([FromQuery] string? pageToken = null)
        {
            var calendars = await _calendarManager.GetCalendarsAsync(pageToken ?? string.Empty);
            var options = (calendars.Items ?? [])
                .Where(calendar => string.Equals(calendar.AccessRole, "owner", StringComparison.OrdinalIgnoreCase))
                .Select(calendar => new CalendarOption
                {
                    Id = calendar.Id,
                    Name = calendar.Summary ?? calendar.Id
                })
                .ToArray();
            return Ok(options);
        }

        [HttpGet("bank-info")]
        public async Task<IActionResult> GetBankInfo()
        {
            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            var accounts = await _dbContext.BankIdentity
                .AsNoTracking()
                .Where(bank => bank.UserId == uid)
                .Select(bank => new BankAccountInfo
                {
                    Id = bank.Id,
                    IBAN = bank.IBAN,
                    BIC = bank.BIC,
                    BankCode = bank.BankCode,
                    WicketCode = bank.WicketCode,
                    AccountNumber = bank.AccountNumber,
                    BankedKey = bank.BankedKey
                })
                .ToArrayAsync();
            return Ok(accounts);
        }

        [HttpPost("bank-info")]
        public async Task<IActionResult> AddBankInfo([FromBody] BankAccountInfoRequest request)
        {
            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            if (!IsValidBankAccountRequest(request))
            {
                return BadRequest(new { error = "Provide an IBAN or BIC and ensure each bank field fits its allowed length." });
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == uid);
            if (user is null)
            {
                return NotFound();
            }

            var bank = new Yavsc.Models.Bank.BankIdentity
            {
                UserId = uid,
                User = user,
                IBAN = request.IBAN?.Trim() ?? string.Empty,
                BIC = request.BIC?.Trim() ?? string.Empty,
                BankCode = request.BankCode?.Trim() ?? string.Empty,
                WicketCode = request.WicketCode?.Trim() ?? string.Empty,
                AccountNumber = request.AccountNumber?.Trim() ?? string.Empty,
                BankedKey = request.BankedKey
            };
            _dbContext.BankIdentity.Add(bank);
            await _dbContext.SaveChangesAsync(uid);
            return Ok(new BankAccountInfo
            {
                Id = bank.Id,
                IBAN = bank.IBAN,
                BIC = bank.BIC,
                BankCode = bank.BankCode,
                WicketCode = bank.WicketCode,
                AccountNumber = bank.AccountNumber,
                BankedKey = bank.BankedKey
            });
        }

        [HttpPut("bank-info/{id:long}")]
        public async Task<IActionResult> UpdateBankInfo(long id, [FromBody] BankAccountInfoRequest request)
        {
            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            if (!IsValidBankAccountRequest(request))
            {
                return BadRequest(new { error = "Provide an IBAN or BIC and ensure each bank field fits its allowed length." });
            }

            var bank = await _dbContext.BankIdentity
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == uid);
            if (bank is null)
            {
                return NotFound();
            }

            bank.IBAN = request.IBAN?.Trim() ?? string.Empty;
            bank.BIC = request.BIC?.Trim() ?? string.Empty;
            bank.BankCode = request.BankCode?.Trim() ?? string.Empty;
            bank.WicketCode = request.WicketCode?.Trim() ?? string.Empty;
            bank.AccountNumber = request.AccountNumber?.Trim() ?? string.Empty;
            bank.BankedKey = request.BankedKey;
            await _dbContext.SaveChangesAsync(uid);
            return NoContent();
        }

        private static bool IsValidBankAccountRequest(BankAccountInfoRequest? request) =>
            request is not null
            && (!string.IsNullOrWhiteSpace(request.IBAN) || !string.IsNullOrWhiteSpace(request.BIC))
            && (request.IBAN?.Length ?? 0) <= 33
            && (request.BIC?.Length ?? 0) <= 15
            && (request.BankCode?.Length ?? 0) <= 5
            && (request.WicketCode?.Length ?? 0) <= 5
            && (request.AccountNumber?.Length ?? 0) <= 15
            && request.BankedKey >= 0;

        [HttpDelete("bank-info/{id:long}")]
        public async Task<IActionResult> DeleteBankInfo(long id)
        {
            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            var bank = await _dbContext.BankIdentity
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == uid);
            if (bank is null)
            {
                return NotFound();
            }

            _dbContext.BankIdentity.Remove(bank);
            await _dbContext.SaveChangesAsync(uid);
            return NoContent();
        }

        [HttpGet("performer-profile")]
        public async Task<IActionResult> GetPerformerProfile()
        {
            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            var profile = await _dbContext.Performers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PerformerId == uid);

            var selectedActivityCodes = await _dbContext.UserActivities
                .AsNoTracking()
                .Where(x => x.UserId == uid)
                .Select(x => x.DoesCode)
                .ToListAsync();

            return Ok(new PerformerProfileSettings
            {
                PerformerId = uid,
                UserName = User.Identity?.Name ?? string.Empty,
                ExerciseCountryCode = profile?.ExerciseCountryCode ?? "fr",
                SIREN = profile?.SIREN ?? string.Empty,
                WebSite = profile?.WebSite ?? string.Empty,
                Active = profile?.Active ?? false,
                AcceptNotifications = profile?.AcceptNotifications ?? false,
                AcceptPublicContact = profile?.AcceptPublicContact ?? false,
                UseGeoLocalizationToReduceDistanceWithClients = profile?.UseGeoLocalizationToReduceDistanceWithClients ?? false,
                SelectedActivityCodes = selectedActivityCodes
            });
        }

        [HttpPut("performer-profile")]
        public async Task<IActionResult> SavePerformerProfile([FromBody] PerformerProfileSettings model)
        {
            if (model is null)
            {
                return BadRequest(new { error = "Payload is required." });
            }

            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(model.PerformerId))
            {
                model.PerformerId = uid;
            }

            if (!string.Equals(model.PerformerId, uid, StringComparison.Ordinal))
            {
                return Forbid();
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == uid);
            if (user is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.ExerciseCountryCode))
            {
                model.ExerciseCountryCode = "fr";
            }

            var profile = await _dbContext.Performers.FirstOrDefaultAsync(p => p.PerformerId == uid);
            if (profile is null)
            {
                profile = new Yavsc.Models.Workflow.PerformerProfile
                {
                    PerformerId = uid,
                    Performer = user,
                    ExerciseCountryCode = model.ExerciseCountryCode,
                    SIREN = model.SIREN ?? string.Empty,
                    WebSite = model.WebSite ?? string.Empty,
                    Active = model.Active,
                    AcceptNotifications = model.AcceptNotifications,
                    AcceptPublicContact = model.AcceptPublicContact,
                    UseGeoLocalizationToReduceDistanceWithClients = model.UseGeoLocalizationToReduceDistanceWithClients,
                    OrganizationAddress = new Yavsc.Models.Relationship.Location()
                };
                _dbContext.Performers.Add(profile);
            }
            else
            {
                profile.ExerciseCountryCode = model.ExerciseCountryCode;
                profile.SIREN = model.SIREN ?? string.Empty;
                profile.WebSite = model.WebSite ?? string.Empty;
                profile.Active = model.Active;
                profile.AcceptNotifications = model.AcceptNotifications;
                profile.AcceptPublicContact = model.AcceptPublicContact;
                profile.UseGeoLocalizationToReduceDistanceWithClients = model.UseGeoLocalizationToReduceDistanceWithClients;
            }

            var desiredCodes = (model.SelectedActivityCodes ?? [])
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingLinks = await _dbContext.UserActivities
                .Where(x => x.UserId == uid)
                .ToListAsync();

            foreach (var link in existingLinks)
            {
                if (!desiredCodes.Contains(link.DoesCode, StringComparer.OrdinalIgnoreCase))
                {
                    _dbContext.UserActivities.Remove(link);
                }
            }

            foreach (var code in desiredCodes)
            {
                if (existingLinks.Any(x => string.Equals(x.DoesCode, code, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var activity = await _dbContext.Activities
                    .FirstOrDefaultAsync(a => a.Code == code);

                if (activity is not null)
                {
                    _dbContext.UserActivities.Add(new Yavsc.Models.Workflow.UserActivity
                    {
                        UserId = uid,
                        DoesCode = activity.Code,
                        Weight = 100
                    });
                }
            }

            await _dbContext.SaveChangesAsync(uid);

            if (!User.IsInRole("Performer"))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Performer");
                if (!roleResult.Succeeded)
                {
                    return BadRequest(new { error = "Performer role could not be assigned.", details = roleResult.Errors.Select(e => e.Description) });
                }
            }

            return Ok(new { status = "saved", performer = model });
        }

        [HttpGet("myhost")]
        public IActionResult MyHost ()
        {
            return Ok(new { host = Request.ForwardedFor() });
        }


        /// <summary>
        /// Updates the avatar
        /// </summary>
        /// <returns></returns>
        [HttpPost("set-avatar")]
        public async Task<IActionResult> SetAvatar()
        {
            var user =  await GetUserData(User.GetUserId());
            if (!Request.HasFormContentType)
            {
                return BadRequest(new
                {
                    status = "invalid_request",
                    message = "A multipart/form-data request is required.",
                    acceptedMimeTypes = AcceptedAvatarMimeTypes,
                    maxFileSizeBytes = MaxAvatarSizeBytes,
                    outputFormat = "image/png",
                });
            }

            if (Request.Form.Files.Count != 1)
            {
                return BadRequest(new
                {
                    status = "invalid_file_count",
                    message = "Exactly one file is required.",
                    acceptedMimeTypes = AcceptedAvatarMimeTypes,
                    maxFileSizeBytes = MaxAvatarSizeBytes,
                    outputFormat = "image/png",
                });
            }

            var avatarFile = Request.Form.Files[0];
            if (avatarFile.Length <= 0)
            {
                return BadRequest(new
                {
                    status = "empty_file",
                    message = "The uploaded file is empty.",
                    acceptedMimeTypes = AcceptedAvatarMimeTypes,
                    maxFileSizeBytes = MaxAvatarSizeBytes,
                    outputFormat = "image/png",
                });
            }

            if (avatarFile.Length > MaxAvatarSizeBytes)
            {
                return BadRequest(new
                {
                    status = "file_too_large",
                    message = "Avatar is too large.",
                    acceptedMimeTypes = AcceptedAvatarMimeTypes,
                    maxFileSizeBytes = MaxAvatarSizeBytes,
                    outputFormat = "image/png",
                });
            }

            if (!AcceptedAvatarMimeTypes.Any(m => string.Equals(m, avatarFile.ContentType, StringComparison.OrdinalIgnoreCase)))
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new
                {
                    status = "unsupported_media_type",
                    message = "Unsupported image format.",
                    acceptedMimeTypes = AcceptedAvatarMimeTypes,
                    maxFileSizeBytes = MaxAvatarSizeBytes,
                    outputFormat = "image/png",
                });
            }

            try
            {
                var info = user.ReceiveAvatar(avatarFile, siteSettings);
                await _dbContext.SaveChangesAsync();
                return Ok(new
                {
                    status = "uploaded",
                    message = "Avatar uploaded successfully.",
                    acceptedMimeTypes = AcceptedAvatarMimeTypes,
                    maxFileSizeBytes = MaxAvatarSizeBytes,
                    outputFormat = "image/png",
                    avatar = info,
                });
            }
            catch (MagickException ex)
            {
                _logger.LogWarning(ex, "Avatar upload failed: invalid image data for user {UserId}", user.Id);
                return BadRequest(new
                {
                    status = "invalid_image_data",
                    message = "Image content could not be decoded.",
                    acceptedMimeTypes = AcceptedAvatarMimeTypes,
                    maxFileSizeBytes = MaxAvatarSizeBytes,
                    outputFormat = "image/png",
                });
            }
        }

        [HttpGet("identity")]
        public async Task<IActionResult> Identity()
        {
            return Json(User.Claims.Select(c=>new {c.Type, c.Value}));
        }
    }
}
