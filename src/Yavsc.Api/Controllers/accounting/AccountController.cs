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

        public ApiAccountController(
        ILoggerFactory loggerFactory, ApplicationDbContext dbContext,
        IOptions<SiteSettings> siteSettings, UserManager<ApplicationUser> userManager)
        {
            _logger = loggerFactory.CreateLogger(nameof(ApiAccountController));
            _dbContext = dbContext;
            this.siteSettings = siteSettings.Value;
            _userManager = userManager;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            if (User == null)
                return new BadRequestObjectResult(
                        new { error = "user not found" });
            var uid = User.GetUserId();
            Debug.Assert(uid != null, "uid is null");
            var userData = await GetUserData(uid);
            Debug.Assert(userData != null, "userData is null");
            var user = new Yavsc.Models.Auth.Me(userData.Id, userData.UserName, userData.Email,
            userData.Avatar,
            userData.PostalAddress, userData.DedicatedGoogleCalendar, userData.FullName);
            user.Address = userData.PostalAddress?.Address ?? string.Empty;

            var userRoles = _dbContext.UserRoles.Where(u => u.UserId == uid).Select(r => r.RoleId).ToArray();

            IdentityRole[] roles = _dbContext.Roles.Where(r => userRoles.Contains(r.Id)).ToArray();

            user.Roles = roles.Select(r => r.Name).ToArray();

            return Ok(user);
        }

        private async Task<ApplicationUser> GetUserData(string uid)
        {
            return await _dbContext.Users
                            .Include(u => u.PostalAddress)
                            .Include(u => u.AccountBalance)
                            .FirstAsync(u => u.Id == uid);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] ProfileUpdateRequest request)
        {
            if (request is null)
            {
                return BadRequest(new { error = "Payload is required." });
            }

            var uid = User.GetUserId();
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Challenge();
            }

            var user = await _dbContext.Users
                .Include(u => u.PostalAddress)
                .FirstOrDefaultAsync(u => u.Id == uid);

            if (user is null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.FullName = request.FullName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.Address))
            {
                if (user.PostalAddress is null)
                {
                    user.PostalAddress = new Yavsc.Models.Relationship.Location { Address = request.Address.Trim() };
                }
                else
                {
                    user.PostalAddress.Address = request.Address.Trim();
                }
            }

            await _dbContext.SaveChangesAsync(uid);
            return Ok(new { status = "saved", fullName = user.FullName, address = user.PostalAddress?.Address });
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
            if (string.IsNullOrWhiteSpace(uid) || !string.Equals(model.PerformerId, uid, StringComparison.Ordinal))
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
