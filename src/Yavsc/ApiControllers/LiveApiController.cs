using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Streaming;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/LiveApi")]
    public class LiveApiController : Controller
    {
        private ApplicationDbContext _context;

        public LiveApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/LiveApi
        [HttpGet]
        public IEnumerable<LiveFlow> GetLiveFlow()
        {
            return _context.LiveFlow;
        }

        // GET: api/LiveApi/5
        [HttpGet("{id}", Name = "GetLiveFlow")]
        public async Task<IActionResult> GetLiveFlow([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            LiveFlow liveFlow = await _context.LiveFlow.SingleAsync(m => m.Id == id);

            if (liveFlow == null)
            {
                return HttpNotFound();
            }

            return Ok(liveFlow);
        }

        // PUT: api/LiveApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLiveFlow([FromRoute] long id, [FromBody] LiveFlow liveFlow)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != liveFlow.Id)
            {
                return HttpBadRequest();
            }
            var uid = User.GetUserId();
            if (liveFlow.OwnerId!=uid)
            {
                ModelState.AddModelError("id","This flow isn't yours.");
                return HttpBadRequest(ModelState);
            }
            
            _context.Entry(liveFlow).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(uid);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LiveFlowExists(id))
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

        // POST: api/LiveApi
        [HttpPost]
        public async Task<IActionResult> PostLiveFlow([FromBody] LiveFlow liveFlow)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }
            
            var uid = User.GetUserId();
            liveFlow.OwnerId=uid;

            _context.LiveFlow.Add(liveFlow);
            try
            {
                await _context.SaveChangesAsync(uid);
            }
            catch (DbUpdateException)
            {
                if (LiveFlowExists(liveFlow.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetLiveFlow", new { id = liveFlow.Id }, liveFlow);
        }

        // DELETE: api/LiveApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLiveFlow([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            LiveFlow liveFlow = await _context.LiveFlow.SingleAsync(m => m.Id == id);
            if (liveFlow == null)
            {
                return HttpNotFound();
            }

            var uid = User.GetUserId();
            if (liveFlow.OwnerId!=uid)
            {
                ModelState.AddModelError("id","This flow isn't yours.");
                return HttpBadRequest(ModelState);
            }

            _context.LiveFlow.Remove(liveFlow);
            await _context.SaveChangesAsync(uid);

            return Ok(liveFlow);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LiveFlowExists(long id)
        {
            return _context.LiveFlow.Count(e => e.Id == id) > 0;
        }
    }
}