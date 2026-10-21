using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Yavsc.Controllers
{
    using Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;
    using Models;
    using ViewModels.FrontOffice;
    using Yavsc.Abstract.Streaming;
    using Yavsc.Server.Helpers;
    using Yavsc.Services;

    public class FrontOfficeController : Controller
    {
        readonly ApplicationDbContext _context;
        readonly UserManager<ApplicationUser> _userManager;
        readonly ILogger _logger;
        readonly IStringLocalizer _SR;
        private readonly IBillingService _billing;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILiveProcessor _liveProcessor;

        public FrontOfficeController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IBillingService billing,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider,
            ILiveProcessor liveProcessor,
            IStringLocalizer<FrontOfficeController> SR)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<FrontOfficeController>();
            _SR = SR;
            _billing = billing;
            _dataProtectionProvider = dataProtectionProvider;
            _liveProcessor = liveProcessor;
        }

        public ActionResult Index()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var model = new FrontOfficeIndexViewModel
            {
                EstimateToProduceCount = _context.RdvQueries.Where(c => c.PerformerId == uid && c.EventDate > now &&  c.Status == QueryStatus.Inserted
               && c.ValidationDate == null && !_context.Estimates.Any(e => e.CommandId == c.Id)).Count(),
                EstimateToHonorAsProCount = _context.RdvQueries.Where(c => c.PerformerId == uid && c.EventDate > now &&  (c.Status == QueryStatus.ProAccepted || c.Status == QueryStatus.Accepted)
                && c.ValidationDate == null && _context.Estimates.Any(e => e.CommandId == c.Id )).Count(),
                EstimateToSignAsCliCount = _context.Estimates.Where(e => e.ClientId == uid && (e.Query.Status == QueryStatus.ProAccepted || e.Query.Status == QueryStatus.Accepted)).Count(),

                BillToSignAsCliCount = 0,
                NewPayementsCount = 0
            };
            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Profiles(string id)
        {
            if (id == null)
            {
                throw new NotImplementedException("No Activity code");
            }
            ViewBag.Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Code == id);
            var result = await _context.ListPerformersAsync(_billing, id);
            return View(result);
        }

        [AllowAnonymous]
        public async Task <ActionResult> ListPerformersAsync(string activityCode)
        {
            if (activityCode == null)
            {
                throw new NotImplementedException("No Activity code");
            }
            ViewBag.Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Code == activityCode);
            var result = await _context.ListPerformersAsync(_billing, activityCode);
            return View(result);
        }

        [AllowAnonymous]
        public async Task<ActionResult> PerformerPodcasts(string id, string? token)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            if (!TryValidateToken(id, token, out var expireAt))
                return BadRequest("Ce lien public de podcast a expire ou est invalide.");

            var performer = await _context.Performers
                .Include(p => p.Performer)
                .FirstOrDefaultAsync(p => p.PerformerId == id);

            if (performer == null) return NotFound();

            var podcasts = (await _liveProcessor.ListAsync(HttpContext.RequestAborted))
                .Where(p => string.Equals(p.OwnerId, id, StringComparison.Ordinal))
                .OrderByDescending(p => p.CreatedAt)
                .ToArray();

            ViewBag.PerformerName = performer.Performer?.UserName ?? performer.PerformerId;
            ViewBag.PerformerId = id;
            ViewBag.PermalinkExpiry = expireAt;
            ViewBag.Permalink = Url.Action(nameof(PerformerPodcasts), "FrontOffice", new { id, token = CreateToken(id) }, Request.Scheme, Request.Host.Value);

            return View(podcasts);
        }

        [AllowAnonymous]
        public async Task<ActionResult> PerformerActivity(string id, string activityCode, string? token)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(activityCode)) return NotFound();

            if (!TryValidateToken(id, activityCode, token, out var expireAt))
                return BadRequest("Ce lien public pour cette activité a expire ou est invalide.");

            var performer = await _context.Performers
                .Include(p => p.Performer)
                .FirstOrDefaultAsync(p => p.PerformerId == id);

            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.Code == activityCode);

            if (performer == null || activity == null) return NotFound();

            var profile = (await _context.ListPerformersAsync(_billing, activityCode))
                .Where(p => string.Equals(p.PerformerId, id, StringComparison.Ordinal))
                .ToList();

            ViewBag.Activity = activity;
            ViewBag.PerformerName = performer.Performer?.UserName ?? performer.PerformerId;
            ViewBag.PerformerId = id;
            ViewBag.PermalinkExpiry = expireAt;
            ViewBag.Permalink = Url.Action(nameof(PerformerActivity), "FrontOffice", new { id, activityCode, token = CreateActivityToken(id, activityCode) }, Request.Scheme, Request.Host.Value);

            return View("Profiles", profile);
        }

        [HttpPost]
        [Authorize]
        public IActionResult GeneratePerformerPodcastPermalink(string id)
        {
            var currentUserId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId) || !string.Equals(currentUserId, id, StringComparison.Ordinal))
                return Forbid();

            var token = CreateToken(id);
            return RedirectToAction(nameof(PerformerPodcasts), new { id, token });
        }

        [HttpPost]
        [Authorize]
        public IActionResult GeneratePerformerActivityPermalink(string id, string activityCode)
        {
            var currentUserId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId) || !string.Equals(currentUserId, id, StringComparison.Ordinal))
                return Forbid();

            var token = CreateActivityToken(id, activityCode);
            return RedirectToAction(nameof(PerformerActivity), new { id, activityCode, token });
        }

        private string CreateToken(string performerId)
        {
            var expiresAt = DateTimeOffset.UtcNow.AddHours(24);
            var protector = _dataProtectionProvider.CreateProtector("Yavsc.Org.PerformerPodcastLink");
            var payload = new
            {
                performerId,
                exp = expiresAt.ToUnixTimeSeconds(),
            };
            return protector.Protect(JsonSerializer.Serialize(payload));
        }

        private string CreateActivityToken(string performerId, string activityCode)
        {
            var expiresAt = DateTimeOffset.UtcNow.AddHours(24);
            var protector = _dataProtectionProvider.CreateProtector("Yavsc.Org.PerformerActivityLink");
            var payload = new
            {
                performerId,
                activityCode,
                exp = expiresAt.ToUnixTimeSeconds(),
            };
            return protector.Protect(JsonSerializer.Serialize(payload));
        }

        private bool TryValidateToken(string performerId, string? token, out DateTimeOffset validUntil)
            => TryValidateTokenInternal(performerId, null, token, out validUntil, "Yavsc.Org.PerformerPodcastLink");

        private bool TryValidateToken(string performerId, string activityCode, string? token, out DateTimeOffset validUntil)
            => TryValidateTokenInternal(performerId, activityCode, token, out validUntil, "Yavsc.Org.PerformerActivityLink");

        private bool TryValidateTokenInternal(string performerId, string? activityCode, string? token, out DateTimeOffset validUntil, string protectorName)
        {
            validUntil = DateTimeOffset.MinValue;
            if (string.IsNullOrWhiteSpace(token)) return false;

            try
            {
                var protector = _dataProtectionProvider.CreateProtector(protectorName);
                var payload = JsonDocument.Parse(protector.Unprotect(token));
                var ownerId = payload.RootElement.GetProperty("performerId").GetString();
                var expiry = payload.RootElement.GetProperty("exp").GetInt64();

                if (!string.Equals(ownerId, performerId, StringComparison.Ordinal)) return false;

                if (!string.IsNullOrWhiteSpace(activityCode))
                {
                    if (!payload.RootElement.TryGetProperty("activityCode", out var activityPayload)) return false;
                    if (!string.Equals(activityPayload.GetString(), activityCode, StringComparison.Ordinal)) return false;
                }

                validUntil = DateTimeOffset.FromUnixTimeSeconds(expiry);
                return validUntil > DateTimeOffset.UtcNow;
            }
            catch
            {
                return false;
            }
        }
    }
}
