using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
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

        [HttpPost]
        [Route("~/api/bugsignal")]
        [Produces("application/json")]
        IActionResult Signal(BugReport report)
        {
            // TODO check ApiKey, check duplicate, and save report
            // var obj = JsonConvert.DeserializeObject(report.ExceptionObjectJson);
            _logger.LogError("bug reported : "+report.ExceptionObjectJson);
            if (!ModelState.IsValid)
                return HttpBadRequest();

            var existent = _context.Bug.Include(b=>b.False).FirstOrDefault(
                f=> f.False.ShortName == report.Component  &&
                f.Description == report.ExceptionObjectJson &&
                f.Status != BugStatus.Rejected
            );
            if (existent != null)
                return Ok(existent);
            
                 existent = new Bug {
                     Description = report.ExceptionObjectJson
                 };
                var existentFalse = _context.Feature.FirstOrDefault
                (f=>f.ShortName == report.Component);
                if (existentFalse==null)
                {
                    existentFalse = new Models.IT.Evolution.Feature {
                    ShortName = report.Component,
                    Description = $"Generated by bug report, {DateTime.Now.ToLongDateString()}"
                   };

                    _context.Feature.Add(existentFalse) ;
                }
                existent.FeatureId = existentFalse.Id;
                existent.False = existentFalse;
                _context.SaveChanges();
          

            return CreatedAtRoute("Signal", new { id = existent.Id }, existent);
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
