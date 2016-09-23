using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;

namespace Yavsc.Controllers
{
    using System;
    using Yavsc.Model;
    using Yavsc.Models;
    using Yavsc.Models.Booking;

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
        public IEnumerable<BookQueryProviderView> GetCommands(long maxId=long.MaxValue)
        {
            var uid = User.GetUserId();
            var now = DateTime.Now;
            
            var result = _context.Commands.Include(c => c.Location).
            Include(c => c.Client).Where(c => c.PerformerId == uid && c.Id < maxId && c.EventDate > now).
            Select(c => new BookQueryProviderView
            {
                Client = new ClientProviderView { UserName = c.Client.UserName, UserId = c.ClientId },
                Location = c.Location,
                EventDate = c.EventDate,
                Id = c.Id,
                Previsional = c.Previsional
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
                return HttpBadRequest(ModelState);
            }
            var uid = User.GetUserId();

            BookQuery bookQuery = _context.Commands.Where(c => c.ClientId == uid || c.PerformerId == uid).Single(m => m.Id == id);

            if (bookQuery == null)
            {
                return HttpNotFound();
            }

            return Ok(bookQuery);
        }

        // PUT: api/BookQueryApi/5
        [HttpPut("{id}")]
        public IActionResult PutBookQuery(long id, [FromBody] BookQuery bookQuery)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != bookQuery.Id)
            {
                return HttpBadRequest();
            }
            var uid = User.GetUserId();
            if (bookQuery.ClientId != uid)
                return HttpNotFound();

            _context.Entry(bookQuery).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookQueryExists(id))
                {
                    return HttpNotFound();
                }
                else
                {
                    throw;
                }
            }

            return new HttpStatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/BookQueryApi
        [HttpPost]
        public IActionResult PostBookQuery([FromBody] BookQuery bookQuery)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }
            var uid = User.GetUserId();
            if (bookQuery.ClientId != uid)
            {
                ModelState.AddModelError("ClientId", "You must be the client at creating a book query");
                return new BadRequestObjectResult(ModelState);
            }
            _context.Commands.Add(bookQuery);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (BookQueryExists(bookQuery.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
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
                return HttpBadRequest(ModelState);
            }
            var uid = User.GetUserId();
            BookQuery bookQuery = _context.Commands.Single(m => m.Id == id);

            if (bookQuery == null)
            {
                return HttpNotFound();
            }
            if (bookQuery.ClientId != uid) return HttpNotFound();

            _context.Commands.Remove(bookQuery);
            _context.SaveChanges();

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
            return _context.Commands.Count(e => e.Id == id) > 0;
        }
    }
}