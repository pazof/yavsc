using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Server.Models.Access;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/FileCircleApi")]
    [Authorize]
    public class FileCircleApiController : Controller
    {
        private ApplicationDbContext _context;

        public FileCircleApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/FileCircleApi
        [HttpGet]
        public IEnumerable<CircleAuthorizationToFile> GetCircleAuthorizationToFile()
        {

            var uid = User.GetUserId();
            return _context.CircleAuthorizationToFile
            .Include( m => m.Circle)
            .Where( m => m.Circle.OwnerId == uid);
     
            // .Where( m => (m.Circle.Public || m.Circle.OwnerId == uid || m.Circle.Members.Any(u=>u.MemberId == uid) ));
        }

        // GET: api/FileCircleApi/5
        [HttpGet("{id}", Name = "GetCircleAuthorizationToFile")]
        public async Task<IActionResult> GetCircleAuthorizationToFile([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }
            var uid = User.GetUserId();

            CircleAuthorizationToFile circleAuthorizationToFile = await _context.CircleAuthorizationToFile
            .Include(m => m.Circle)
            .Include( m => m.Circle.Members)
            .SingleOrDefaultAsync(m => m.Circle.OwnerId == uid && m.CircleId == id);

            if (circleAuthorizationToFile == null)
            {
                return HttpNotFound();
            }

            return Ok(circleAuthorizationToFile);
        }

        // PUT: api/FileCircleApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCircleAuthorizationToFile([FromRoute] long id, [FromBody] CircleAuthorizationToFile circleAuthorizationToFile)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != circleAuthorizationToFile.CircleId)
            {
                return HttpBadRequest();
            }
            var uid = User.GetUserId();
            var circle = await _context.Circle.FirstOrDefaultAsync(c=>c.Id == id);
            if (circle == null)
            {
                return HttpBadRequest();
            }

            if (uid != circle.OwnerId)
            {
                return HttpBadRequest();
            }

            _context.Entry(circleAuthorizationToFile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(uid);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CircleAuthorizationToFileExists(id))
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

        // POST: api/FileCircleApi
        [HttpPost]
        public async Task<IActionResult> PostCircleAuthorizationToFile([FromBody] CircleAuthorizationToFile circleAuthorizationToFile)
        {
            
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }
            var uid = User.GetUserId();
            var circle = await _context.Circle.FirstOrDefaultAsync(c=>c.Id == circleAuthorizationToFile.CircleId);
            if (circle == null || circle.OwnerId != uid)
            {
                return HttpBadRequest();
            }


            _context.CircleAuthorizationToFile.Add(circleAuthorizationToFile);
            try
            {
                await _context.SaveChangesAsync(uid);
            }
            catch (DbUpdateException)
            {
                if (CircleAuthorizationToFileExists(circleAuthorizationToFile.CircleId))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetCircleAuthorizationToFile", new { id = circleAuthorizationToFile.CircleId }, circleAuthorizationToFile);
        }

        // DELETE: api/FileCircleApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCircleAuthorizationToFile([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            CircleAuthorizationToFile circleAuthorizationToFile = await _context.CircleAuthorizationToFile
            .Include(c=>c.Circle).SingleAsync(m => m.CircleId == id);
            if (circleAuthorizationToFile == null)
            {
                return HttpNotFound();
            }

            var uid = User.GetUserId();
            if (circleAuthorizationToFile == null || circleAuthorizationToFile.Circle.OwnerId != uid)
            {
                return HttpBadRequest();
            }

            _context.CircleAuthorizationToFile.Remove(circleAuthorizationToFile);
            await _context.SaveChangesAsync();

            return Ok(circleAuthorizationToFile);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CircleAuthorizationToFileExists(long id)
        {
            return _context.CircleAuthorizationToFile.Count(e => e.CircleId == id) > 0;
        }
    }
}