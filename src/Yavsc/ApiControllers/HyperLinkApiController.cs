using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Relationship;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/hyperlink")]
    public class HyperLinkApiController : Controller
    {
        private ApplicationDbContext _context;

        public HyperLinkApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/HyperLinkApi
        [HttpGet]
        public IEnumerable<HyperLink> GetLinks()
        {
            return _context.Links;
        }

        // GET: api/HyperLinkApi/5
        [HttpGet("{id}", Name = "GetHyperLink")]
        public async Task<IActionResult> GetHyperLink([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            HyperLink hyperLink = await _context.Links.SingleAsync(m => m.HRef == id);

            if (hyperLink == null)
            {
                return HttpNotFound();
            }

            return Ok(hyperLink);
        }

        // PUT: api/HyperLinkApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHyperLink([FromRoute] string id, [FromBody] HyperLink hyperLink)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != hyperLink.HRef)
            {
                return HttpBadRequest();
            }

            _context.Entry(hyperLink).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HyperLinkExists(id))
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

        // POST: api/HyperLinkApi
        [HttpPost]
        public async Task<IActionResult> PostHyperLink([FromBody] HyperLink hyperLink)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.Links.Add(hyperLink);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (HyperLinkExists(hyperLink.HRef))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetHyperLink", new { id = hyperLink.HRef }, hyperLink);
        }

        // DELETE: api/HyperLinkApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHyperLink([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            HyperLink hyperLink = await _context.Links.SingleAsync(m => m.HRef == id);
            if (hyperLink == null)
            {
                return HttpNotFound();
            }

            _context.Links.Remove(hyperLink);
            await _context.SaveChangesAsync();

            return Ok(hyperLink);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool HyperLinkExists(string id)
        {
            return _context.Links.Count(e => e.HRef == id) > 0;
        }
    }
}
