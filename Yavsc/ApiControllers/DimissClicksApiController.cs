using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Messaging;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/dimiss")]
    public class DimissClicksApiController : Controller
    {
        private ApplicationDbContext _context;

        public DimissClicksApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DimissClicksApi
        [HttpGet]
        public IEnumerable<DimissClicked> GetDimissClicked()
        {
            var uid = User.GetUserId();
            return _context.DimissClicked.Where(d=>d.UserId == uid);
        }

        [HttpGet("click/{noteid}"),AllowAnonymous]
        public async Task<IActionResult> Click(long noteid )
        {
            if (User.IsSignedIn())
            return await PostDimissClicked(new DimissClicked { NotificationId= noteid, UserId = User.GetUserId()});
            await HttpContext.Session.LoadAsync();
            var clicked = HttpContext.Session.GetString("clicked");
            if (clicked == null) {
                HttpContext.Session.SetString("clicked",noteid.ToString());
            } else HttpContext.Session.SetString("clicked",$"{clicked}:{noteid}");
            await HttpContext.Session.CommitAsync();
            return Ok();
        }
        // GET: api/DimissClicksApi/5
        [HttpGet("{id}", Name = "GetDimissClicked")]
        public async Task<IActionResult> GetDimissClicked([FromRoute] string id)
        {
            var uid = User.GetUserId();
            if (uid != id) return new ChallengeResult();

            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            DimissClicked dimissClicked = await _context.DimissClicked.SingleAsync(m => m.UserId == id);

            if (dimissClicked == null)
            {
                return HttpNotFound();
            }

            return Ok(dimissClicked);
        }

        // PUT: api/DimissClicksApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDimissClicked([FromRoute] string id, [FromBody] DimissClicked dimissClicked)
        {
            var uid = User.GetUserId();
            if (uid != id || uid != dimissClicked.UserId) return new ChallengeResult();

            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != dimissClicked.UserId)
            {
                return HttpBadRequest();
            }

            _context.Entry(dimissClicked).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DimissClickedExists(id))
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

        // POST: api/DimissClicksApi
        [HttpPost]
        public async Task<IActionResult> PostDimissClicked([FromBody] DimissClicked dimissClicked)
        {
            var uid = User.GetUserId();
            if (uid != dimissClicked.UserId) return new ChallengeResult();

            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.DimissClicked.Add(dimissClicked);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DimissClickedExists(dimissClicked.UserId))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetDimissClicked", new { id = dimissClicked.UserId }, dimissClicked);
        }

        // DELETE: api/DimissClicksApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDimissClicked([FromRoute] string id)
        {
            var uid = User.GetUserId();
            if (!User.IsInRole("Administrator"))
            if (uid != id) return new ChallengeResult();
            
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            DimissClicked dimissClicked = await _context.DimissClicked.SingleAsync(m => m.UserId == id);
            if (dimissClicked == null)
            {
                return HttpNotFound();
            }

            _context.DimissClicked.Remove(dimissClicked);
            await _context.SaveChangesAsync();

            return Ok(dimissClicked);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DimissClickedExists(string id)
        {
            return _context.DimissClicked.Count(e => e.UserId == id) > 0;
        }
    }
}