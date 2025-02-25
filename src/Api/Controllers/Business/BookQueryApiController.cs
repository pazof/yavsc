using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Yavsc.Controllers
{
    using System;
    using Yavsc.Models;
    using Yavsc.Models.Workflow;
    using Yavsc.Models.Billing;
    using Yavsc.Abstract.Identity;
    using Microsoft.EntityFrameworkCore;
    using Yavsc.Helpers;

    [Produces("application/json")]
    [Route("api/bookquery"), Authorize(Roles = "Performer,Administrator")]
    public class BookQueryApiController : Controller
    {
        private ApplicationDbContext _context;
        private ILogger _logger;

        public BookQueryApiController(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<BookQueryApiController>();
        }

         // GET: api/BookQueryApi
         /// <summary>
        /// Book queries, by creation order
        /// </summary>
        /// <param name="maxId">returned Ids must be lower than this value</param>
        /// <returns>book queries</returns>
        [HttpGet]
        public IEnumerable<RdvQueryProviderInfo> GetCommands(long maxId=long.MaxValue)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.Now;
            
            var result = _context.RdvQueries.Include(c => c.Location).
            Include(c => c.Client).Where(c => c.PerformerId == uid && c.Id < maxId && c.EventDate > now
            && c.ValidationDate == null).
            Select(c => new RdvQueryProviderInfo
            {
                Client = new ClientProviderInfo {
                     UserName = c.Client.UserName,
                     UserId = c.ClientId, 
                     Avatar = c.Client.Avatar },
                Location = c.Location,
                EventDate = c.EventDate,
                Id = c.Id,
                Previsional = c.Previsional,
                Reason = c.Reason,
                ActivityCode = c.ActivityCode,
                BillingCode = BillingCodes.Rdv
            }).
            OrderBy(c=>c.Id).
            Take(25);
            return result;
        }

        // GET: api/BookQueryApi/5
        [HttpGet("{id}", Name = "GetBookQuery")]
        public IActionResult GetBookQuery([FromRoute] long id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);

            RdvQuery bookQuery = _context.RdvQueries.Where(c => c.ClientId == uid || c.PerformerId == uid).Single(m => m.Id == id);

            if (bookQuery == null)
            {
                return NotFound();
            }

            return Ok(bookQuery);
        }

        // PUT: api/BookQueryApi/5
        [HttpPut("{id}")]
        public IActionResult PutBookQuery(long id, [FromBody] RdvQuery bookQuery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != bookQuery.Id)
            {
                return BadRequest();
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (bookQuery.ClientId != uid)
                return NotFound();

            _context.Entry(bookQuery).State = EntityState.Modified;

            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookQueryExists(id))
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

        // POST: api/BookQueryApi
        [HttpPost]
        public IActionResult PostBookQuery([FromBody] RdvQuery bookQuery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (bookQuery.ClientId != uid)
            {
                ModelState.AddModelError("ClientId", "You must be the client at creating a book query");
                return new BadRequestObjectResult(ModelState);
            }
            _context.RdvQueries.Add(bookQuery);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (BookQueryExists(bookQuery.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetBookQuery", new { id = bookQuery.Id }, bookQuery);
        }

        // DELETE: api/BookQueryApi/5
        [HttpDelete("{id}")]
        public IActionResult DeleteBookQuery(long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            RdvQuery bookQuery = _context.RdvQueries.Single(m => m.Id == id);

            if (bookQuery == null)
            {
                return NotFound();
            }
            if (bookQuery.ClientId != uid) return NotFound();

            _context.RdvQueries.Remove(bookQuery);
            _context.SaveChanges(User.GetUserId());

            return Ok(bookQuery);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BookQueryExists(long id)
        {
            return _context.RdvQueries.Count(e => e.Id == id) > 0;
        }
    }
}
