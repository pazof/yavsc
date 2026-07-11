using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Security.Claims;
using Yavsc.Helpers;
using Yavsc.ViewModels;
using Yavsc.Models.Billing;
using Yavsc.Server.Models.FileSystem;

namespace Yavsc.ApiControllers
{
    using Models;
    using Services;

    using Models.Messaging;
    using Microsoft.Extensions.Options;
    using Microsoft.EntityFrameworkCore;
    using Yavsc.ViewModels.Auth;
    using Yavsc.Server.Helpers;

    [Route("api/bill"), Authorize]
    public class BillingController : Controller
    {
        readonly ApplicationDbContext dbContext;
        private readonly IStringLocalizer _localizer;
        private readonly GoogleAuthSettings _googleSettings;
        private readonly IYavscMessageSender _GCMSender;
        private readonly IAuthorizationService authorizationService;


        private readonly ILogger logger;
        private readonly IBillingService billingService;

        public BillingController(
            IAuthorizationService authorizationService,
            ILoggerFactory loggerFactory,
            IStringLocalizer<BillingController> SR,
            ApplicationDbContext context,
            IOptions<GoogleAuthSettings> googleSettings,
            IYavscMessageSender GCMSender,
            IBillingService billingService
            )
        {
            _googleSettings=googleSettings.Value;
            this.authorizationService = authorizationService;
            dbContext = context;
            logger = loggerFactory.CreateLogger<BillingController>();
            this._localizer = SR;
            _GCMSender=GCMSender;
            this.billingService=billingService;
        }

        [HttpGet("facture-{billingCode}-{id}.pdf"), Authorize]
        public async Task<IActionResult> GetPdf(string billingCode, long id)
        {
            var bill = await billingService.GetBillAsync(billingCode, id);

            if ( authorizationService.AuthorizeAsync(User, bill, new ReadPermission()).IsFaulted)
            {
                return new ChallengeResult();
            }

            var fi = bill.GetBillInfo(billingService);

            if (!fi.Exists) return Ok(new { Error = "Not generated" });
            return File(fi.OpenRead(), "application/x-pdf", fi.Name);
        }

        [HttpGet("facture-{billingCode}-{id}.tex"), Authorize]
        public async Task<IActionResult> GetTex(string billingCode, long id)
        {
            var bill = await billingService.GetBillAsync(billingCode, id);

            if (bill==null) {
               logger.LogCritical ( $"# not found !! {id} in {billingCode}");
               return this.NotFound();
            }
            logger.LogTrace(JsonConvert.SerializeObject(bill));

            if (!(await authorizationService.AuthorizeAsync(User, bill, new ReadPermission())).Succeeded)
            {
                return new ChallengeResult();
            }
            Response.ContentType = "text/x-tex";
            return ViewComponent("Bill",new object[] {  billingCode, bill , OutputFormat.LaTeX, true });
        }

        [HttpPost("genpdf/{billingCode}/{id}")]
        public async Task<IActionResult> GeneratePdf(string billingCode, long id)
        {
            var bill = await billingService.GetBillAsync(billingCode, id);

            if (bill==null) {
               logger.LogCritical ( $"# not found !! {id} in {billingCode}");
               return this.NotFound();
            }
             logger.LogWarning("Got bill ack:"+bill.GetIsAcquitted().ToString());
            return ViewComponent("Bill",new object[] { billingCode, bill, OutputFormat.Pdf, true } );
        }


        [HttpPost("prosign/{billingCode}/{id}")]
        public async Task<IActionResult> ProSign(string billingCode, long id)
        {
            var estimate = dbContext.Estimates.
            Include(e=>e.Client).Include(e=>e.Client.DeviceDeclaration)
            .Include(e=>e.Bill).Include(e=>e.Owner).Include(e=>e.Owner.Performer)
            .FirstOrDefault(e=>e.Id == id);
            if (estimate == null)
                return new BadRequestResult();
            if (!(await authorizationService.AuthorizeAsync(User, estimate, new ReadPermission())).Succeeded)


            {
                return new ChallengeResult();
            }
            if (Request.Form.Files.Count!=1)
                return new BadRequestResult();
            await User.ReceiveProSignatureAsync(billingCode,id,Request.Form.Files[0],"pro");
            estimate.ProviderValidationDate = DateTime.UtcNow;
            dbContext.SaveChanges(User.GetUserId());
            // Notify the client
            var locstr = _localizer["EstimationMessageToClient"];

            var yaev = new EstimationEvent(estimate,_localizer);

            var regids = new [] { estimate.Client.Id };
            bool gcmSent = false;
                var grep = await _GCMSender.NotifyEstimateAsync(regids,yaev);
                gcmSent = grep.success>0;
            return Ok (new { ProviderValidationDate = estimate.ProviderValidationDate, GCMSent = gcmSent });
        }

        [HttpGet("prosign/{billingCode}/{id}")]
        public async Task<IActionResult> GetProSign(string billingCode, long id)
        {
            // For authorization purpose
            var estimate = dbContext.Estimates.FirstOrDefault(e=>e.Id == id);
            if (!(await authorizationService.AuthorizeAsync(User, estimate, new ReadPermission())).Succeeded)

            {
                return new ChallengeResult();
            }

            var filename = AbstractFileSystemHelpers.SignFileNameFormat("pro", billingCode, id);
            FileInfo fi = new FileInfo(Path.Combine(AbstractFileSystemHelpers.UserBillsDirName, filename));
            if (!fi.Exists) return NotFound(new { Error = "Professional signature not found" });
            return File(fi.OpenRead(), "application/x-pdf", filename); ;
        }

        [HttpPost("clisign/{billingCode}/{id}")]
        public async Task<IActionResult> CliSign(string billingCode, long id)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var estimate = dbContext.Estimates.Include( e=>e.Query
            ).Include(e=>e.Owner).Include(e=>e.Owner.Performer).Include(e=>e.Client)
            .FirstOrDefault( e=> e.Id == id && e.Query.ClientId == uid );
            if (!(await authorizationService.AuthorizeAsync(User, estimate, new ReadPermission())).Succeeded)
            {
                return new ChallengeResult();
            }
            if (Request.Form.Files.Count!=1)
                return new BadRequestResult();
            await User.ReceiveProSignatureAsync(billingCode,id,Request.Form.Files[0],"cli");
            estimate.ClientValidationDate = DateTime.UtcNow;
            dbContext.SaveChanges(User.GetUserId());
            return Ok (new { ClientValidationDate = estimate.ClientValidationDate });
        }

        [HttpGet("clisign/{billingCode}/{id}")]
        public async Task<IActionResult> GetCliSign(string billingCode, long id)
        {
            // For authorization purpose
            var estimate = dbContext.Estimates.FirstOrDefault(e=>e.Id == id);
            if (!(await authorizationService.AuthorizeAsync(User, estimate, new ReadPermission())).Succeeded)
            {
                return new ChallengeResult();
            }

            var filename = AbstractFileSystemHelpers.SignFileNameFormat("pro", billingCode, id);
            FileInfo fi = new FileInfo(Path.Combine(AbstractFileSystemHelpers.UserBillsDirName, filename));
            if (!fi.Exists) return NotFound(new { Error = "Professional signature not found" });
            return File(fi.OpenRead(), "application/x-pdf", filename); ;
        }

        /// <summary>
        /// Capture a signature for an estimate, in the JSON
        /// wire format produced by PostIt (see
        /// <c>PostIt.Models.SignaturePadData</c>). The legacy
        /// <c>POST prosign</c> / <c>POST clisign</c> endpoints
        /// take a PNG <see cref="IFormFile"/>; this one takes a
        /// JSON body so the capture happens entirely in-app on
        /// the client side, without a rasterisation step.
        ///
        /// <para>The route is intentionally a sibling of the
        /// legacy endpoints, not a replacement: the legacy
        /// PNG-based flow stays in place to keep the TeX
        /// invoice templates (<c>Bill_tex.cshtml</c>,
        /// <c>Estimate_tex.cshtml</c>) working until the
        /// migration commit regenerates PNGs from the JSON
        /// payload. The two flows share the
        /// <see cref="Signature"/> table for storage but not
        /// the URL surface.</para>
        /// </summary>
        [HttpPost("estimate/{id:long}/sign")]
        [ValidateAntiForgeryToken]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Sign(
            [FromRoute] long id,
            [FromBody] SignatureSubmission body,
            CancellationToken token)
        {
            if (body is null) return BadRequest(new { Error = "missing body" });
            if (body.Strokes is null) return BadRequest(new { Error = "missing strokes" });
            if (string.IsNullOrEmpty(body.SignerUserId))
                return BadRequest(new { Error = "missing signerUserId" });

            var estimate = await dbContext.Estimates
                .Include(e => e.Client)
                .FirstOrDefaultAsync(e => e.Id == id, token);
            if (estimate is null) return NotFound(new { Error = "estimate not found" });

            // The signer is identified by userId in the body, not
            // by the bearer token, because the OAuth scope we
            // carry is for the API client (PostIt), not the end
            // user. We trust the body's userId to match either
            // Owner or Client, and reject everything else.
            var userId = body.SignerUserId;
            if (userId != estimate.OwnerId && userId != estimate.ClientId)
                return Forbid();

            // Map userId → type. The Pro/Client split is the
            // same one the legacy prosign/clisign endpoints use;
            // keeping the rule here means the Signature table
            // and the legacy ProviderValidationDate/ClientValidationDate
            // columns can co-exist without contradicting each other.
            var type = userId == estimate.OwnerId
                ? SignatureType.Pro
                : SignatureType.Client;

            var payload = new SignaturePadPayload
            {
                CoordinateMax = body.CoordinateMax,
                CapturedAtUtc = body.CapturedAtUtc ?? DateTime.UtcNow,
                Strokes = body.Strokes,
            };

            // Disk write first: a disk failure shouldn't leave
            // a Signature row pointing at a file that doesn't
            // exist. The file helper throws on filesystem
            // problems and propagates here.
            FileReceivedInfo fi;
            try
            {
                fi = await User.ReceiveEstimateSignatureAsync(id, type, payload, token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "estimate {Id}: signature file write failed", id);
                return BadRequest(new { Error = "file write failed", Detail = ex.Message });
            }

            // Find-or-add: the (EstimateId, Type) pair is
            // unique, so a second POST for the same side of the
            // estimate replaces the previous signature. EF
            // translates this into a single UPDATE when the
            // row exists and an INSERT otherwise; the unique
            // index in ApplicationDbContext is the
            // database-level guarantee that the contract
            // holds if two requests race.
            var signature = await dbContext.Signatures
                .FirstOrDefaultAsync(s => s.EstimateId == id && s.Type == type, token);

            if (signature is null)
            {
                signature = new Signature
                {
                    EstimateId = id,
                    SignerId = userId,
                    Type = type,
                };
                dbContext.Signatures.Add(signature);
            }
            else
            {
                // Roll the signer's quota back by the size of
                // the file we're about to orphan: the old
                // FilePath is no longer referenced once we
                // overwrite FilePath below.
                try
                {
                    var orphan = new FileInfo(signature.FilePath);
                    if (orphan.Exists)
                    {
                        var signerForOrphan = await dbContext.Users
                            .FirstOrDefaultAsync(u => u.Id == userId, token);
                        if (signerForOrphan is not null)
                            signerForOrphan.DiskUsage =
                                Math.Max(0, signerForOrphan.DiskUsage - orphan.Length);
                    }
                }
                catch { /* best effort — the file is being replaced anyway */ }
            }

            signature.SignerId = userId;
            signature.CoordinateMax = payload.CoordinateMax;
            signature.Strokes = payload.Strokes;
            signature.CapturedAtUtc = payload.CapturedAtUtc;
            signature.FilePath = Path.Combine(fi.DestDir, fi.FileName);

            // Bump the signer's quota. The Signature row's
            // SignerId is the IdentityUser.Id (a string), so we
            // look up by Id and not by username.
            var signer = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId, token);
            if (signer is not null)
            {
                signer.DiskUsage += new FileInfo(signature.FilePath).Length;
            }

            try
            {
                await dbContext.SaveChangesAsync(token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "estimate {Id}: signature db write failed", id);
                // Best-effort rollback: remove the file we wrote
                // so disk and db don't disagree.
                try { System.IO.File.Delete(signature.FilePath); }
                catch { /* swallow — the row will be re-orphaned, the user re-signs */ }
                return BadRequest(new { Error = "db write failed", Detail = ex.Message });
            }

            var location = Url.Action(nameof(Sign), new { id })
                ?? $"/api/bill/estimate/{id}/sign";
            return Created(location, new
            {
                id = signature.Id,
                estimateId = signature.EstimateId,
                type = signature.Type.ToString(),
                capturedAtUtc = signature.CapturedAtUtc,
                coordinateMax = signature.CoordinateMax,
            });
        }
    }
}

/// <summary>
/// JSON body of <c>POST /api/bill/estimate/{id}/sign</c>. The
/// shape mirrors what PostIt sends; the <c>signerUserId</c>
/// field disambiguates which side of the estimate signed
/// because the bearer token belongs to the PostIt OAuth
/// client, not the end user.
/// </summary>
public class SignatureSubmission
{
    /// <summary>
    /// ApplicationUser.Id of the signer. Must equal
    /// <c>Estimate.OwnerId</c> for a Pro signature or
    /// <c>Estimate.ClientId</c> for a Client signature.
    /// </summary>
    public string SignerUserId { get; set; }

    /// <summary>
    /// Wire-format strokes. See
    /// <c>PostIt.Models.SignaturePadData</c>.
    /// </summary>
    public int[] Strokes { get; set; } = Array.Empty<int>();

    public int CoordinateMax { get; set; } = 10_000;

    /// <summary>
    /// Client-reported capture time. The server may override
    /// this with <c>DateTime.UtcNow</c> if the client is
    /// caught lying about clock skew, but the default is to
    /// trust the client.
    /// </summary>
    public DateTime? CapturedAtUtc { get; set; }
}
