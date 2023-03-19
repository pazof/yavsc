using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;


namespace Yavsc.ApiControllers
{
    using Yavsc;
    using System;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.Extensions.Logging;
    using Models;
    using Services;
    using Models.Haircut;
    using System.Threading.Tasks;
    using Helpers;
    using Models.Payment;
    using Newtonsoft.Json;
    using PayPal.PayPalAPIInterfaceService.Model;
    using Yavsc.Models.Haircut.Views;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Authorization;

    [Route("api/haircut")][Authorize]
    public class HairCutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        public HairCutController(ApplicationDbContext context,
          ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<HairCutController>();
        }

        // GET: api/HairCutQueriesApi
        // Get the active queries for current
        // user, as a client
        public IActionResult Index()
        {
            
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var now = DateTime.Now;
            var result = _context.HairCutQueries
                         .Include(q => q.Prestation)
                         .Include(q => q.Client)
                         .Include(q => q.PerformerProfile)
                         .Include(q => q.Location)
                         .Where(
                q => q.ClientId == uid
                && ( q.EventDate > now || q.EventDate == null )
                && q.Status == QueryStatus.Inserted
                ).Select(q => new HaircutQueryClientInfo(q));
            return Ok(result);
        }

        // GET: api/HairCutQueriesApi/5
        [HttpGet("{id}", Name = "GetHairCutQuery")]
        public async Task<IActionResult> GetHairCutQuery([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            HairCutQuery hairCutQuery = await _context.HairCutQueries.SingleAsync(m => m.Id == id);

            if (hairCutQuery == null)
            {
                return NotFound();
            }

            return Ok(hairCutQuery);
        }

        // PUT: api/HairCutQueriesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHairCutQuery([FromRoute] long id, [FromBody] HairCutQuery hairCutQuery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != hairCutQuery.Id)
            {
                return BadRequest();
            }

            _context.Entry(hairCutQuery).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HairCutQueryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        [HttpPost]
        public async Task<IActionResult> PostQuery(HairCutQuery hairCutQuery)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.HairCutQueries.Add(hairCutQuery);
            try
            {
                await _context.SaveChangesAsync(uid);
            }
            catch (DbUpdateException)
            {
                if (HairCutQueryExists(hairCutQuery.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetHairCutQuery", new { id = hairCutQuery.Id }, hairCutQuery);

        }

        [HttpPost("createpayment/{id}")]
        public async Task<IActionResult> CreatePayment(long id)
        {

            HairCutQuery query = await _context.HairCutQueries.Include(q => q.Client).
            Include(q => q.Client.PostalAddress).Include(q => q.Prestation).Include(q=>q.Regularisation)
            .SingleAsync(q => q.Id == id);
            if (query.PaymentId!=null)
                return new BadRequestObjectResult(new { error = "An existing payment process already exists" });
            query.SelectedProfile = _context.BrusherProfile.Single(p => p.UserId == query.PerformerId);
            SetExpressCheckoutResponseType payment = null;
            try {
                payment = Request.CreatePayment("HairCutCommand", query,  "sale", _logger);
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
                return new StatusCodeResult(500);
            }

            if (payment==null) {
                _logger.LogError("Error doing SetExpressCheckout, aborting.");
                _logger.LogError(JsonConvert.SerializeObject(Startup.PayPalSettings));
                return new StatusCodeResult(500);
            }
            switch (payment.Ack)
            {
                case AckCodeType.SUCCESS:
                case AckCodeType.SUCCESSWITHWARNING:
                    {
                        var dbinfo = new PayPalPayment
                        {
                            ExecutorId = User.GetUserId(),
                            CreationToken = payment.Token,
                            State = payment.Ack.ToString()
                        };
                        await _context.SaveChangesAsync(User.GetUserId());
                    }
                    break;

                default:
                    _logger.LogError(JsonConvert.SerializeObject(payment));
                    return new BadRequestObjectResult(payment);
            }

            return Json(new { token = payment.Token });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHairCutQuery([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return  BadRequest(ModelState);
            }

            HairCutQuery hairCutQuery = await _context.HairCutQueries.SingleAsync(m => m.Id == id);
            if (hairCutQuery == null)
            {
                return NotFound();
            }

            _context.HairCutQueries.Remove(hairCutQuery);
            await _context.SaveChangesAsync();

            return Ok(hairCutQuery);
        }

        private bool HairCutQueryExists(long id)
        {
            return _context.HairCutQueries.Count(e => e.Id == id) > 0;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
