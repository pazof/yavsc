using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Yavsc.Models;
using Yavsc.Models.Streaming;
using Yavsc.ViewModels.Streaming;

namespace Yavsc.Controllers
{
    [Route("api/live")]
    public class LiveApiController : Controller
    {
        public static ConcurrentDictionary<string, LiveCastMeta> Casters = new ConcurrentDictionary<string, LiveCastMeta>();

        private ApplicationDbContext _dbContext;
        ILogger _logger;

        /// <summary>
        /// Live Api Controller
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="context"></param>

        public LiveApiController(
            ILoggerFactory loggerFactory,
            ApplicationDbContext context)
        {
            _dbContext = context;
            _logger = loggerFactory.CreateLogger<LiveApiController>();
        }
        [HttpGet("filenamehint/{id}")]
        public async Task<string[]> GetFileNameHint(string id)
        {
            return await _dbContext.Tags.Where( t=> t.Name.StartsWith(id)).Select(t=>t.Name).Take(25).ToArrayAsync();
        }
        
        [HttpGet("live/{id}")]
        public async Task<IActionResult> GetLive(string id)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest) return new BadRequestResult();
            var uid = User.GetUserId();
            var existent = Casters[id];
            var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            if (existent.Listeners.TryAdd(uid,socket)) {
                 return Ok();
            }
            else {
                await socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable,"Listeners.TryAdd failed",CancellationToken.None);
            }
            return HttpBadRequest("Listeners.TryAdd returned false");
        }

        /// <summary>
        /// Lists user's live castings
        /// </summary>
        /// <param name="meta/id">user id</param>
        /// <returns></returns>
        public IActionResult Index(long? id)
        {
            if (id==0)
            return View("Index", Casters.Select(c=> new { UserName = c.Key, Listenning = c.Value.Listeners.Count }));

            var flow = _dbContext.LiveFlow.SingleOrDefault(f=>f.Id == id);
            if (flow == null) return HttpNotFound();

            return View("Flow", flow);

        }


        [HttpGet("meta/{id}")]
        public async Task<IActionResult> GetLiveFlow([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            LiveFlow liveFlow = await _dbContext.LiveFlow.SingleAsync(m => m.Id == id);

            if (liveFlow == null)
            {
                return HttpNotFound();
            }

            return Ok(liveFlow);
        }

        [HttpPut("meta/{id}")]
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

            _dbContext.Entry(liveFlow).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync(uid);
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

        [HttpPost("meta")]
        public async Task<IActionResult> PostLiveFlow([FromBody] LiveFlow liveFlow)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }
            
            var uid = User.GetUserId();
            liveFlow.OwnerId=uid;

            _dbContext.LiveFlow.Add(liveFlow);
            try
            {
                await _dbContext.SaveChangesAsync(uid);
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
        [HttpDelete("meta/{id}")]
        public async Task<IActionResult> DeleteLiveFlow([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            LiveFlow liveFlow = await _dbContext.LiveFlow.SingleAsync(m => m.Id == id);
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

            _dbContext.LiveFlow.Remove(liveFlow);
            await _dbContext.SaveChangesAsync(uid);

            return Ok(liveFlow);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LiveFlowExists(long id)
        {
            return _dbContext.LiveFlow.Count(e => e.Id == id) > 0;
        }
    }
}
