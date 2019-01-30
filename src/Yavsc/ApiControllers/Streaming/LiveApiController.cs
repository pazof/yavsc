using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Yavsc.Helpers;
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
        public async Task<string[]> GetFileNameHint(string id)
        {
            return await _dbContext.Tags.Where( t=> t.Name.StartsWith(id)).Select(t=>t.Name).Take(25).ToArrayAsync();
        }
        
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

        public async Task<IActionResult> Cast(long id)
        {
            // ensure this request is for a websocket
            if (!HttpContext.WebSockets.IsWebSocketRequest) return new BadRequestResult();

            var uname = User.GetUserName();
            // ensure uniqueness of casting stream from this user
            var existent = Casters[uname];
            if (existent != null)  
            { 
                ModelState.AddModelError("error","not supported, you already casting, there's support for one live streaming only");
                return new BadRequestObjectResult(ModelState);
            }
            var uid = User.GetUserId();
            // get some setup from user
            var flow = _dbContext.LiveFlow.Include(f=>f.Owner).SingleOrDefault(f=> (f.OwnerId==uid && f.Id == id));
            if (flow == null)
            {
                ModelState.AddModelError("error",$"You don't own any flow with the id {id}");
                return new BadRequestObjectResult (ModelState);
            }
              
            // Accept the socket
            var meta = new LiveCastMeta { Socket = await HttpContext.WebSockets.AcceptWebSocketAsync() };
            // Dispatch the flow
            using (meta.Socket)
            {
                if (meta.Socket != null && meta.Socket.State == WebSocketState.Open)
                {
                    Casters[uname] = meta;
                    // TODO: Handle the socket here.
                    // Find receivers: others in the chat room
                    // send them the flow

                    byte[] buffer = new byte[1024];
                    WebSocketReceiveResult received = await meta.Socket.ReceiveAsync
                    (new ArraySegment<byte>(buffer), CancellationToken.None);

                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    
                    hubContext.Clients.All.addPublicStream(new { id = flow.Id, sender = flow.Owner.UserName, title = flow.Title, url = flow.GetFileUrl(), 
                        mediaType = flow.MediaType }, $"{flow.Owner.UserName} is starting a stream!");

                    // FIXME do we really need to close those one in invalid state ?
                    Stack<string> ToClose = new Stack<string>();

                    while (received.MessageType != WebSocketMessageType.Close)
                    {
                        _logger.LogInformation($"Echoing {received.Count} bytes received in a {received.MessageType} message; Fin={received.EndOfMessage}");
                        // Echo anything we receive
                        // and send to all listner found
                        foreach (var cliItem in meta.Listeners)
                        {
                            var listenningSocket = cliItem.Value;
                            if (listenningSocket.State == WebSocketState.Open)
                                await listenningSocket.SendAsync(new ArraySegment<byte>
                                (buffer, 0, received.Count), received.MessageType, received.EndOfMessage, CancellationToken.None);
                            else ToClose.Push(cliItem.Key);
                        }
                        received = await meta.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        string no;
                        do
                        {
                            no = ToClose.Pop();
                            WebSocket listenningSocket;
                            if (meta.Listeners.TryRemove(no, out listenningSocket))
                                await listenningSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "State != WebSocketState.Open", CancellationToken.None);

                        } while (no != null);
                    }
                    await meta.Socket.CloseAsync(received.CloseStatus.Value, received.CloseStatusDescription, CancellationToken.None);
                    Casters[uname] = null;
                }
                else _logger.LogInformation($"failed (meta.Socket != null && meta.Socket.State == WebSocketState.Open)");
            }

            return Ok();
        }


        /// <summary>
        /// Lists user's live castings
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns></returns>
        public IActionResult Index(long? id)
        {
            if (id==0)
            return View("Index", Casters.Select(c=> new { UserName = c.Key, Listenning = c.Value.Listeners.Count }));

            var flow = _dbContext.LiveFlow.SingleOrDefault(f=>f.Id == id);
            if (flow == null) return HttpNotFound();

            return View("Flow", flow);

        }


        [HttpGet("{id}", Name = "GetLiveFlow")]
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

        [HttpPost]
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
        [HttpDelete("{id}")]
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