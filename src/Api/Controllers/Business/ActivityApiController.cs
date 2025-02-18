using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Api.Helpers;
using Yavsc.Models;
using Yavsc.Models.Workflow;


namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/activity")]
    [AllowAnonymous]
    public class ActivityApiController : Controller
    {
        private ApplicationDbContext _context;

        public ActivityApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ActivityApi
        [HttpGet]
        public IEnumerable<Activity> GetActivities()
        {
            return _context.Activities.Include(a=>a.Forms).Where( a => !a.Hidden );
        }

        // GET: api/ActivityApi/5
        [HttpGet("{id}", Name = "GetActivity")]
        public async Task<IActionResult> GetActivity([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Activity activity = await _context.Activities.SingleAsync(m => m.Code == id);

            if (activity == null)
            {
                return NotFound();
            }
            // Also return hidden ones
            // hidden doesn't mean disabled
            return Ok(activity);
        }

        // PUT: api/ActivityApi/5
        [HttpPut("{id}"),Authorize("AdministratorOnly")]
        public async Task<IActionResult> PutActivity([FromRoute] string id, [FromBody] Activity activity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != activity.Code)
            {
                return BadRequest();
            }

            _context.Entry(activity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityExists(id))
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

        // POST: api/ActivityApi
        [HttpPost,Authorize("AdministratorOnly")]
        public async Task<IActionResult> PostActivity([FromBody] Activity activity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Activities.Add(activity);
            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (ActivityExists(activity.Code))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetActivity", new { id = activity.Code }, activity);
        }

        // DELETE: api/ActivityApi/5
        [HttpDelete("{id}"),Authorize("AdministratorOnly")]
        public async Task<IActionResult> DeleteActivity([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Activity activity = await _context.Activities.SingleAsync(m => m.Code == id);
            if (activity == null)
            {
                return NotFound();
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync(User.GetUserId());

            return Ok(activity);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ActivityExists(string id)
        {
            return _context.Activities.Count(e => e.Code == id) > 0;
        }
    }
}
