using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Yavsc.ViewModels.Streaming;
using Yavsc.Models;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Yavsc.Controllers.Communicating
{
    public class LiveController : Controller
    {
        ILogger _logger;
        ApplicationDbContext _dbContext;
        public static ConcurrentDictionary<string, LiveCastMeta> Casters = new ConcurrentDictionary<string, LiveCastMeta>();

        /// <summary>
        /// Controls the live !!!
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="dbContext"></param>
        public LiveController(ILoggerFactory loggerFactory, 
        ApplicationDbContext dbContext)
        {
            _logger = loggerFactory.CreateLogger<LiveController>();
            _dbContext = dbContext;
        }
        public IActionResult Index(long? id)
        {
            if (id==0)
            return View("Index", Casters.Select(c=> new { UserName = c.Key, Listenning = c.Value.Listeners.Count }));

            var flow = _dbContext.LiveFlow.SingleOrDefault(f=>f.Id == id);
            if (flow == null) return HttpNotFound();

            return View("Flow", flow);

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
            if (existent != null) return new BadRequestObjectResult("not supported, you already casting, there's support for one live streaming only");
            var uid = User.GetUserId();
            // get some setup from user
            var flow = _dbContext.LiveFlow.SingleOrDefault(f=> (f.OwnerId==uid && f.Id == id));
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
    }
}