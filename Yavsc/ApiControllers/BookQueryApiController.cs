using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Booking;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/BookQueryApi")]
    public class BookQueryApiController : Controller
    {
        private ApplicationDbContext _context;

        public BookQueryApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BookQueryApi
        [HttpGet]
        public IEnumerable<BookQuery> GetCommands()
        {
            return _context.Commands;
        }

        // GET: api/BookQueryApi/5
        [HttpGet("{id}", Name = "GetBookQuery")]
        public IActionResult GetBookQuery([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            BookQuery bookQuery = _context.Commands.Single(m => m.Id == id);

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

            BookQuery bookQuery = _context.Commands.Single(m => m.Id == id);
            if (bookQuery == null)
            {
                return HttpNotFound();
            }

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