using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Workflow;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Services;
using Yavsc.Server.Helpers;
using Yavsc.ViewModels.FrontOffice;

namespace Yavsc.ApiControllers
{
    [Authorize]
    [Route(Constants.APIPrefix + "/front")]
    public class FrontOfficeApiController : Controller
    {
        ApplicationDbContext dbContext;
        private readonly IBillingService billing;
        private readonly ILogger logger;

        public FrontOfficeApiController(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IBillingService billing = null)
        {
            dbContext = context;
            this.billing = billing ?? new BillingService(context);
            logger = loggerFactory.CreateLogger<FrontOfficeApiController>();
        }

        [HttpGet("profiles/{actCode}")]
        async Task <IEnumerable<PerformerProfileViewModel>> Profiles(string actCode)
        {
            return await dbContext.ListPerformersAsync(billing, actCode);
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

                var type = role == ActorRole.Provider
                    ? SignatureType.Pro
                    : SignatureType.Client;

                var payload = new SignaturePadPayload
                {
                    CoordinateMax = body!.CoordinateMax,
                    CapturedAtUtc = body.CapturedAtUtc ?? DateTime.UtcNow,
                    Strokes = body.Strokes!,
                };

                try
                {
                    var uid = User.GetUserId();
                    signature = await EstimateSignaturePersister.StageAsync(
                        dbContext, estimate.Id, type, uid, payload, token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "query {QueryId}: signature stage failed", queryId);
                    return BadRequest(new { Error = "signature stage failed", Detail = ex.Message });
                }

                if (role == ActorRole.Provider)
                    estimate.ProviderValidationDate = DateTime.UtcNow;
                else
                    estimate.ClientValidationDate = DateTime.UtcNow;
            }

            // The party-specific status records who accepted: the
            // provider (ProAccepted) or the client (ClientAccepted).
            // An admin accepting (neither party) falls back to the
            // legacy generic Accepted.
            query.Status = role switch
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

namespace Yavsc.ViewModels.FrontOffice
{
    /// <summary>
    /// Body of <c>POST /api/v1/front/query/accept</c> and
    /// <c>.../reject</c>. The <see cref="Strokes"/> field is optional:
    /// when present (non-empty) on accept, it is the PostIt wire-format
    /// signature captured with the acceptance. All fields are optional
    /// so a caller may post <c>{}</c> to accept/reject without a
    /// signature.
    /// </summary>
    public class QueryAcceptanceRequest
    {
        /// <summary>
        /// Wire-format strokes. See
        /// <c>PostIt.Models.SignaturePadData</c>. Null or empty means
        /// no signature.
        /// </summary>
        public int[]? Strokes { get; set; }

        public int CoordinateMax { get; set; } = 10_000;

        public DateTime? CapturedAtUtc { get; set; }
    }
}