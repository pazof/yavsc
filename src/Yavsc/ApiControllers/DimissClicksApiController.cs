using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Messaging;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/dimiss")]
    public class DimissClicksApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DimissClicksApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DimissClicksApi
        [HttpGet]
        public IEnumerable<DismissClicked> GetDismissClicked()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return _context.DismissClicked.Where(d=>d.UserId == uid);
        }

        [HttpGet("click/{noteid}"),AllowAnonymous]
        public async Task<IActionResult> Click(long noteid )
        {
            if (User.IsSignedIn())
            return await PostDismissClicked(new DismissClicked { NotificationId= noteid, UserId = User.GetUserId()});
            await HttpContext.Session.LoadAsync();
            var clicked = HttpContext.Session.GetString("clicked");
            if (clicked == null) {
                HttpContext.Session.SetString("clicked",noteid.ToString());
            } else HttpContext.Session.SetString("clicked",$"{clicked}:{noteid}");
            await HttpContext.Session.CommitAsync();
            return Ok();
        }
        // GET: api/DimissClicksApi/5
        [HttpGet("{id}", Name = "GetDismissClicked")]
        public async Task<IActionResult> GetDismissClicked([FromRoute] string id)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid != id) return new ChallengeResult();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DismissClicked DismissClicked = await _context.DismissClicked.SingleAsync(m => m.UserId == id);

            if (DismissClicked == null)
            {
                return NotFound();
            }

            return Ok(DismissClicked);
        }

        // PUT: api/DimissClicksApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDismissClicked([FromRoute] string id, [FromBody] DismissClicked DismissClicked)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid != id || uid != DismissClicked.UserId) return new ChallengeResult();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != DismissClicked.UserId)
            {
                return BadRequest();
            }

            _context.Entry(DismissClicked).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DismissClickedExists(id))
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

        // POST: api/DimissClicksApi
        [HttpPost]
        public async Task<IActionResult> PostDismissClicked([FromBody] DismissClicked DismissClicked)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid != DismissClicked.UserId) return new ChallengeResult();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.DismissClicked.Add(DismissClicked);
            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (DismissClickedExists(DismissClicked.UserId))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetDismissClicked", new { id = DismissClicked.UserId }, DismissClicked);
        }

        // DELETE: api/DimissClicksApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDismissClicked([FromRoute] string id)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Administrator"))
            if (uid != id) return new ChallengeResult();
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DismissClicked DismissClicked = await _context.DismissClicked.SingleAsync(m => m.UserId == id);
            if (DismissClicked == null)
            {
                return NotFound();
            }

            _context.DismissClicked.Remove(DismissClicked);
            await _context.SaveChangesAsync(User.GetUserId());

            return Ok(DismissClicked);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DismissClickedExists(string id)
        {
            return _context.DismissClicked.Count(e => e.UserId == id) > 0;
        }
    }
}
