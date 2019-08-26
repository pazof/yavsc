using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Yavsc.Models;
using Yavsc.Models.IT.Fixing;

namespace Yavsc.ApiControllers
{
    [Produces("application/json")]
    [Route("~/api/bug")]
    public class BugApiController : Controller
    {
        private ApplicationDbContext _context;
        ILogger _logger;

        public BugApiController(ApplicationDbContext context, ILoggerFactory factory)
        {
            _logger = factory.CreateLogger<BugApiController>();
            _context = context;
        }

        [HttpPost("signal")]
        [Authorize]
        public IActionResult Signal([FromBody] BugReport report)
        {
            _logger.LogWarning("bug reported : "+report.ExceptionObjectJson);
            if (report.ExceptionObjectJson!=null)
                if (report.ExceptionObjectJson.Length > 10240)
                    report.ExceptionObjectJson = report.ExceptionObjectJson.Substring(0,10240);
            if (!ModelState.IsValid)

            {
                _logger.LogError("Bag request :");
                _logger.LogError(JsonConvert.SerializeObject(ModelState));
                return new BadRequestObjectResult(ModelState);
            }
            var existent = _context.Bug.FirstOrDefault( b =>
                b.Description == report.ExceptionObjectJson &&
                b.Title == report.Component &&
                b.Status != BugStatus.Rejected
            );
            if (existent != null)
            {
                _logger.LogInformation("bug already reported");
                return Ok(existent);
            }
            _logger.LogInformation("Filling a new bug report");
            var bug = new Bug {
                 Title = report.Component,
                 Description = report.ExceptionObjectJson
            };
            _context.Bug.Add(bug);
            _context.SaveChanges();
            return CreatedAtRoute("GetBug", new { id = bug.Id }, bug);
        }

        // GET: api/BugApi
        [HttpGet]
        public IEnumerable<Bug> GetBug()
        {
            return _context.Bug;
        }

        // GET: api/bug/5
        [HttpGet("{id}", Name = "GetBug")]
        public async Task<IActionResult> GetBug([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            Bug bug = await _context.Bug.SingleAsync(m => m.Id == id);

            if (bug == null)
            {
                return HttpNotFound();
            }

            return Ok(bug);
        }

        // PUT: api/BugApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBug([FromRoute] long id, [FromBody] Bug bug)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != bug.Id)
            {
                return HttpBadRequest();
            }

            _context.Entry(bug).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BugExists(id))
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

        // POST: api/bug
        [HttpPost]
        public async Task<IActionResult> PostBug([FromBody] Bug bug)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.Bug.Add(bug);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BugExists(bug.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetBug", new { id = bug.Id }, bug);
        }

        // DELETE: api/BugApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBug([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            Bug bug = await _context.Bug.SingleAsync(m => m.Id == id);
            if (bug == null)
            {
                return HttpNotFound();
            }

            _context.Bug.Remove(bug);
            await _context.SaveChangesAsync();

            return Ok(bug);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BugExists(long id)
        {
            return _context.Bug.Count(e => e.Id == id) > 0;
        }
    }
}
