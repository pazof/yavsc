using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using ImageMagick;

using Yavsc.Models;
using Yavsc.Api.Helpers;
using Yavsc.Server.Helpers;
using System.Diagnostics;

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
        private readonly ILogger _logger;

        public ApiAccountController(
        ILoggerFactory loggerFactory, ApplicationDbContext dbContext)
        {
            _logger = loggerFactory.CreateLogger(nameof(ApiAccountController));
            _dbContext = dbContext;
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
            userData.PostalAddress, userData.DedicatedGoogleCalendar);

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
                var info = user.ReceiveAvatar(avatarFile);
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
