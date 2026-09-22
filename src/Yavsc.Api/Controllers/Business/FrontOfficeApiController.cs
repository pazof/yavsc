using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Workflow;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Services;
using Yavsc.Server.Helpers;
using Yavsc.ViewModels.FrontOffice;
using Yavsc.ViewModels.Gen;

namespace Yavsc.ApiControllers
{
    [Authorize]
    [Route(Constants.APIPrefix + "/front")]
    public class FrontOfficeApiController : Controller
    {
        ApplicationDbContext dbContext;
        private readonly IBillingService billing;
        private readonly ILogger logger;
        private readonly SiteSettings siteSettings;
        private readonly IRazorViewEngine viewEngine;

        // The view-rendering dependencies (siteSettings, viewEngine) are
        // optional: only EstimateTex/EstimatePdf use them, and those run in
        // the Api host where Program.cs wires AddControllersWithViews +
        // SiteSettings. The accept/reject endpoints (and the Api integration
        // test host, which composes with AddControllers alone) never touch
        // them, so they must not be required for controller activation.
        //
        // .NET 10 does not register a single IViewEngine service (the view
        // engine is registered as IRazorViewEngine and surfaced through
        // MvcViewOptions.ViewEngines). IRazorViewEngine (RazorViewEngine)
        // implements IViewEngine, so we inject that and hand it to
        // TeXHelpers.RenderViewToString as an IViewEngine. RenderViewToString
        // builds its ActionContext from the controller's own ControllerContext,
        // so no IActionContextAccessor is needed (and that type is deprecated
        // in .NET 10).
        public FrontOfficeApiController(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IOptions<SiteSettings> siteSettings = null,
            IRazorViewEngine viewEngine = null,
            IBillingService billing = null)
        {
            dbContext = context;
            this.billing = billing ?? new BillingService(context);
            logger = loggerFactory.CreateLogger<FrontOfficeApiController>();
            this.siteSettings = siteSettings?.Value;
            this.viewEngine = viewEngine;
        }

        [HttpGet("profiles/{actCode}")]
        async Task <IEnumerable<PerformerProfileViewModel>> Profiles(string actCode)
        {
            return await dbContext.ListPerformersAsync(billing, actCode);
        }

        // GET: api/front/query/{queryId}/estimate.tex
        //
        // Renders the estimate linked to the query (Estimate.CommandId ==
        // queryId) as a LaTeX source document. The view is a generation
        // template (Layout = "null"), not a web page: it is returned with
        // a text/x-tex content type so the caller can save it as a .tex
        // file. Either party (client or provider) or an admin may read it.
        [HttpGet("query/{queryId}/estimate.tex")]
        public async Task<IActionResult> EstimateTex(long queryId, CancellationToken token)
        {
            if (viewEngine is null || siteSettings is null)
                return Problem("TeX rendering is not configured on this host.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);

            var estimate = await LoadEstimateForRenderAsync(queryId, token);
            if (estimate is null) return NotFound(new { Error = "no estimate linked to this query" });

            var uid = User.GetUserId();
            if (!User.IsInRole(Constants.AdminGroupName)
                && uid != estimate.ClientId && uid != estimate.OwnerId)
                return Forbid();

            PrepareEstimateViewBag(estimate);
            Response.ContentType = "text/x-tex";
            return View("Estimate_tex", estimate);
        }

        // GET: api/front/query/{queryId}/estimate.pdf
        //
        // Renders the same LaTeX template to a string, compiles it to PDF
        // via texi2pdf (see SiteSettings.GenerateEstimatePdf), and streams
        // the generated PDF bytes back. Requires /usr/bin/texi2pdf on the
        // server host; on hosts without it the endpoint returns a problem.
        [HttpGet("query/{queryId}/estimate.pdf")]
        public async Task<IActionResult> EstimatePdf(long queryId, CancellationToken token)
        {
            if (viewEngine is null || siteSettings is null)
            {
                string msg = "PDF rendering is not configured on this host.";
                logger.LogError(msg);
                         return Problem(msg,
                    statusCode: StatusCodes.Status503ServiceUnavailable);

            }

            var estimate = await LoadEstimateForRenderAsync(queryId, token);
            if (estimate is null) return NotFound(new { Error = "no estimate linked to this query" });

            var uid = User.GetUserId();
            if (!User.IsInRole(Constants.AdminGroupName)
                && uid != estimate.ClientId && uid != estimate.OwnerId)
                return Forbid();

            PrepareEstimateViewBag(estimate);

            var baseFileName = $"estimate-{estimate.Id}";
            // Resolve the bills dir to an absolute path once: appsettings
            // configures it as a relative path ("bills"), and both the
            // generation (GenerateEstimatePdf) and the read-back below must
            // agree on the same location regardless of the host's cwd.
            var billsDir = new System.IO.DirectoryInfo(siteSettings.Bills).FullName;
            var model = new PdfGenerationViewModel
            {
                BaseFileName = baseFileName,
                DestDir = billsDir,
                Temp = siteSettings.TempDir,
            };

            try
            {
                // 1. Render the LaTeX template (Estimate_tex) to a TeX source
                //    string, using the estimate as the view model.
                model.TeXSource = this.RenderViewToString(viewEngine, "Estimate_tex", estimate);
                // 2. Render the generation view (Estimate_pdf). Its @{} block
                //    calls SiteSettings.GenerateEstimatePdf(Model), which
                //    compiles the TeX to PDF on disk as a side effect of the
                //    render. The HTML the view emits is not what we return —
                //    we stream the generated PDF bytes — but driving the
                //    generation through the view keeps the template as the
                //    output producer, per the original design.
                if (model.GenerateEstimatePdf() is FileInfo fo && fo.Exists)
                {
                     return File(fo.OpenRead(), "application/pdf");
                }
                string msg = "TeX render failed: " + model.GenerationErrorMessage;
                logger.LogError(msg);
                return Problem(msg);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "estimate {QueryId}: TeX/PDF render failed", queryId);
                return Problem("TeX render failed: " + ex.Message);
            }
      }

        // Loads the estimate linked to a query with the navigation the
        // Estimate_tex template reads (client + performer profile + bill
        // + the captured signatures, drawn as tikz at the foot of the devis).
        private Task<Estimate?> LoadEstimateForRenderAsync(long queryId, CancellationToken token)
            => dbContext.Estimates
                .Where(e => e.CommandId == queryId)
                .OrderByDescending(e => e.Id)
                .Include(e => e.Bill)
                .Include(e => e.Signatures)
                .Include(e => e.Query).ThenInclude(q => q!.Client).ThenInclude(c => c!.PostalAddress)
                .Include(e => e.Query).ThenInclude(q => q!.PerformerProfile).ThenInclude(p => p!.OrganizationAddress)
                .Include(e => e.Query).ThenInclude(q => q!.PerformerProfile).ThenInclude(p => p!.Performer)
                .FirstOrDefaultAsync(token);

        // Sets the ViewBag keys the Estimate_tex template reads. A devis
        // is never a bill (AsBill=false) and is never acquitted.
        private void PrepareEstimateViewBag(Estimate estimate)
        {
            ViewBag.AsBill = false;
            ViewBag.Acquitted = false;
            ViewBag.BillsDir = new System.IO.DirectoryInfo(siteSettings.Bills).FullName;
            ViewBag.AvatarsDir = new System.IO.DirectoryInfo(siteSettings.Avatars).FullName;
        }

        // POST: api/front/query/accept
        //
        // Either party of the query may accept: the provider
        // (PerformerId) accepts the client's request, or the client
        // (ClientId) accepts the provider's estimate. An optional
        // signature (the PostIt wire format) may be sent along; it is
        // persisted on the estimate linked to the query
        // (Estimate.CommandId == queryId), and the matching validation
        // date is stamped — ProviderValidationDate when the provider
        // accepts, ClientValidationDate when the client accepts.
        [HttpPost("query/accept")]
        public async Task<IActionResult> AcceptQuery(
            string billingCode,
            long queryId,
            [FromBody] QueryAcceptanceRequest? body,
            CancellationToken token)
        {
            var (query, role, error) = await ResolveAndAuthorizeAsync(billingCode, queryId, token);
            if (error is not null) return error;
            if (query is null) return BadRequest(new { Error = "query not found" });

            Signature? signature = null;
            // actingRole is the role the caller is signing / accepting
            // as. It defaults to the role inferred from the caller's
            // identity, but when a signature is submitted the caller
            // declares the side (Pro/Client) and the server honors it
            // only if the caller is that party. This disambiguates a
            // user who is both parties (where inference would always
            // pick the provider) and makes "write only for its author"
            // explicit.
            ActorRole actingRole = role;
            if (HasSignature(body))
            {
                // Admins bypass authorization but are neither party of
                // the estimate, so they cannot sign.
                if (role == ActorRole.Admin)
                    return BadRequest(new { Error = "admin cannot sign on behalf of a party" });

                var estimate = await dbContext.Estimates
                    .Where(e => e.CommandId == queryId)
                    .OrderByDescending(e => e.Id)
                    .FirstOrDefaultAsync(token);
                if (estimate is null)
                    return BadRequest(new { Error = "no estimate linked to this query" });

                var uid = User.GetUserId();
                bool isProvider = query.PerformerId == uid;
                bool isClient = query.ClientId == uid;

                // The caller declares which side they sign as. Null
                // falls back to the role inferred from identity.
                var declared = body!.SignatureType;
                if (declared.HasValue)
                {
                    if (declared.Value == SignatureType.Pro && !isProvider)
                        return Forbid();
                    if (declared.Value == SignatureType.Client && !isClient)
                        return Forbid();
                    actingRole = declared.Value == SignatureType.Pro
                        ? ActorRole.Provider
                        : ActorRole.Client;
                }

                var type = actingRole == ActorRole.Provider
                    ? SignatureType.Pro
                    : SignatureType.Client;

                var payload = new SignaturePadPayload
                {
                    CoordinateMax = body.CoordinateMax,
                    CapturedAtUtc = body.CapturedAtUtc ?? DateTime.UtcNow,
                    Strokes = body.Strokes!,
                };

                try
                {
                    signature = await EstimateSignaturePersister.StageAsync(
                        dbContext, estimate.Id, type, uid, payload, token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "query {QueryId}: signature stage failed", queryId);
                    return BadRequest(new { Error = "signature stage failed", Detail = ex.Message });
                }

                if (actingRole == ActorRole.Provider)
                    estimate.ProviderValidationDate = DateTime.UtcNow;
                else
                    estimate.ClientValidationDate = DateTime.UtcNow;
            }

            // The party-specific status records who accepted: the
            // provider (ProAccepted) or the client (ClientAccepted).
            // An admin accepting (neither party) falls back to the
            // legacy generic Accepted.
            query.Status = actingRole switch
            {
                ActorRole.Provider => QueryStatus.ProAccepted,
                ActorRole.Client => QueryStatus.ClientAccepted,
                _ => QueryStatus.Accepted,
            };

            try
            {
                await dbContext.SaveChangesAsync(User.GetUserId(), token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "query {QueryId}: accept db write failed", queryId);
                return BadRequest(new { Error = "db write failed", Detail = ex.Message });
            }

            return Ok(new
            {
                queryId,
                status = query.Status.ToString(),
                signature = signature is null
                    ? null
                    : new
                    {
                        id = signature.Id,
                        estimateId = signature.EstimateId,
                        type = signature.Type.ToString(),
                    },
            });
        }

        // POST: api/front/query/reject
        //
        // Either party may reject. A signature is not meaningful on a
        // rejection, so one is refused with 400.
        [HttpPost("query/reject")]
        public async Task<IActionResult> RejectQuery(
            string billingCode,
            long queryId,
            [FromBody] QueryAcceptanceRequest? body,
            CancellationToken token)
        {
            if (HasSignature(body))
                return BadRequest(new { Error = "signature not allowed on reject" });

            var (query, role, error) = await ResolveAndAuthorizeAsync(billingCode, queryId, token);
            if (error is not null) return error;
            if (query is null) return BadRequest(new { Error = "query not found" });

            query.Status = QueryStatus.Rejected;
            await dbContext.SaveChangesAsync(User.GetUserId(), token);

            return Ok(new
            {
                queryId,
                status = query.Status.ToString(),
            });
        }

        // Loads the query and checks that the caller is a party to it
        // (provider or client) or an admin. Returns the role the caller
        // is acting in; Admin is only returned when the caller is
        // neither party but is an admin.
        private async Task<(IQuery? Query, ActorRole Role, IActionResult? Error)>
            ResolveAndAuthorizeAsync(string billingCode, long queryId, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(billingCode))
                return (null, ActorRole.None, BadRequest("billingCode"));
            if (queryId == 0)
                return (null, ActorRole.None, BadRequest("queryId"));

            var query = await billing.GetBillAsync(billingCode, queryId);
            if (query is null)
                return (null, ActorRole.None, BadRequest());

            var uid = User.GetUserId();
            bool isProvider = query.PerformerId == uid;
            bool isClient = query.ClientId == uid;

            if (isProvider) return (query, ActorRole.Provider, null);
            if (isClient) return (query, ActorRole.Client, null);

            if (User.IsInRole(Constants.AdminGroupName))
                return (query, ActorRole.Admin, null);

            return (null, ActorRole.None, Forbid());
        }

        private static bool HasSignature(QueryAcceptanceRequest? body)
            => body is not null && body.Strokes is not null && body.Strokes.Length > 0;
    }

    internal enum ActorRole { None, Provider, Client, Admin }
}
