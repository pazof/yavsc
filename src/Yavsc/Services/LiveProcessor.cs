using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.ViewModels.Streaming;
using Yavsc.Models.Messaging;
using Yavsc.Models.FileSystem;
using Newtonsoft.Json;

namespace Yavsc.Services
{

    public class LiveProcessor : ILiveProcessor {
        IHubContext _hubContext;
        private ILogger _logger;
        ApplicationDbContext _dbContext;
        public PathString LiveCastingPath {get; set;} = Constants.LivePath;


        public  ConcurrentDictionary<string, LiveCastHandler> Casters {get;} = new ConcurrentDictionary<string, LiveCastHandler>();

        public LiveProcessor(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            _logger = loggerFactory.CreateLogger<LiveProcessor>();
        }

        public async Task<bool> AcceptStream (HttpContext context)
        {
            // TODO defer request handling
            var liveId = long.Parse(context.Request.Path.Value.Substring(LiveCastingPath.Value.Length + 1));
            var userId = context.User.GetUserId();
            var user = await _dbContext.Users.FirstAsync(u => u.Id == userId);
            var uname = user.UserName;
            var flow = _dbContext.LiveFlow.Include(f => f.Owner).SingleOrDefault(f => (f.OwnerId == userId && f.Id == liveId));
            if (flow == null)
            {
                _logger.LogWarning("Aborting. Flow info was not found.");
                context.Response.StatusCode = 400;
                return false;
            }
            _logger.LogInformation("flow : "+flow.Title+" for "+uname);
            LiveCastHandler meta = null;
            if (Casters.ContainsKey(uname))
            {
                _logger.LogWarning($"Casters.ContainsKey({uname})");
                meta = Casters[uname];
                if (meta.Socket.State == WebSocketState.Open || meta.Socket.State == WebSocketState.Connecting )
                {
                _logger.LogWarning($"Closing cx");
                    // FIXME loosed connexion should be detected & disposed else where
                    await meta.Socket.CloseAsync( WebSocketCloseStatus.EndpointUnavailable, "one by user", CancellationToken.None);
                    
                }
                if (!meta.TokenSource.IsCancellationRequested) {
                    meta.TokenSource.Cancel();
                }
                meta.Socket.Dispose();
                meta.Socket = await context.WebSockets.AcceptWebSocketAsync();
                meta.TokenSource = new CancellationTokenSource();
            }
            else
            {
                _logger.LogInformation($"new caster");
                // Accept the socket
                meta = new LiveCastHandler { Socket = await context.WebSockets.AcceptWebSocketAsync() };
            }
            _logger.LogInformation("Accepted web socket");
            // Dispatch the flow
            
            try
            {
                if (meta.Socket != null && meta.Socket.State == WebSocketState.Open)
                {
                    Casters[uname] = meta;
                    // TODO: Handle the socket here.
                    // Find receivers: others in the chat room
                    // send them the flow
                    var buffer = new byte[Constants.WebSocketsMaxBufLen];
                    var sBuffer = new ArraySegment<byte>(buffer);
                    _logger.LogInformation("Receiving bytes...");

                    WebSocketReceiveResult received = await meta.Socket.ReceiveAsync(sBuffer, meta.TokenSource.Token);
                    _logger.LogInformation($"Received bytes : {received.Count}");
                    _logger.LogInformation($"Is the end : {received.EndOfMessage}");
                    const string livePath = "live";

                    string destDir = context.User.InitPostToFileSystem(livePath);
                    _logger.LogInformation($"Saving flow to {destDir}");
                    string fileName =  flow.GetFileName();
                    var fsInputQueue = new Queue<ArraySegment<byte>>();
                    var taskWritingToFs = Task<FileRecievedInfo>.Run( ()=>  user.ReceiveUserFile(destDir, fsInputQueue, fileName, flow.MediaType, meta.TokenSource.Token));
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

                    hubContext.Clients.All.addPublicStream(new PublicStreamInfo
                    {
                        id = flow.Id,
                        sender = flow.Owner.UserName,
                        title = flow.Title,
                        url = flow.GetFileUrl(),
                        mediaType = flow.MediaType
                    }, $"{flow.Owner.UserName} is starting a stream!");

                    Stack<string> ToClose = new Stack<string>();

                    try
                    {
                            
                        _logger.LogInformation($"Echoing {received.Count} bytes received in a {received.MessageType} message; Fin={received.EndOfMessage}");
                        // Echo anything we receive
                        // and send to all listner found
                        foreach (var cliItem in meta.Listeners)
                        {
                            var listenningSocket = cliItem.Value;
                            if (listenningSocket.State == WebSocketState.Open) {
                                await listenningSocket.SendAsync(
                                sBuffer, received.MessageType, received.EndOfMessage, meta.TokenSource.Token);

                            }
                            else
                            if (listenningSocket.State == WebSocketState.CloseReceived || listenningSocket.State == WebSocketState.CloseSent)
                            {
                                ToClose.Push(cliItem.Key);
                            }
                        }
                        fsInputQueue.Enqueue(sBuffer);
                        // logger.LogInformation("replying...");
                        while (!received.CloseStatus.HasValue)
                        {
                            // reply echo
                            // await meta.Socket.SendAsync(new ArraySegment<byte>(buffer), received.MessageType, received.EndOfMessage, meta.TokenSource.Token);
                                                        
                            _logger.LogInformation("Receiving new bytes...");   
                            buffer = new byte[Constants.WebSocketsMaxBufLen];
                            sBuffer = new ArraySegment<byte>(buffer);
                                                                        
                            received = await meta.Socket.ReceiveAsync(sBuffer, meta.TokenSource.Token);
                            foreach (var cliItem in meta.Listeners)
                            {
                                var listenningSocket = cliItem.Value;
                                if (listenningSocket.State == WebSocketState.Open) {
                                    await listenningSocket.SendAsync(
                                    sBuffer, received.MessageType, received.EndOfMessage, meta.TokenSource.Token);
                                }
                                else
                                if (listenningSocket.State == WebSocketState.CloseReceived || listenningSocket.State == WebSocketState.CloseSent)
                                {
                                    ToClose.Push(cliItem.Key);
                                }
                            }
                            fsInputQueue.Enqueue(sBuffer);
                            _logger.LogInformation($"Received new bytes : {received.Count}");
                            _logger.LogInformation($"Is the end : {received.EndOfMessage}");
                             while (ToClose.Count >0)
                            {
                                string no = ToClose.Pop();
                                _logger.LogInformation("Closing follower connection");
                                WebSocket listenningSocket;
                                if (meta.Listeners.TryRemove(no, out listenningSocket)) {
                                    await listenningSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable,
                                     "State != WebSocketState.Open", CancellationToken.None);
                                    listenningSocket.Dispose();
                                }
                            }
                        }
                        _logger.LogInformation("Closing connection");
                        await meta.Socket.CloseAsync(received.CloseStatus.Value, received.CloseStatusDescription, CancellationToken.None);
                        meta.Socket.Dispose();

                        meta.TokenSource.Cancel();
                        taskWritingToFs.Wait();
                        _logger.LogInformation("Resulting file : " +JsonConvert.SerializeObject(taskWritingToFs.Result));

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception occured : {ex.Message}");
                        _logger.LogError(ex.StackTrace);
                        await meta.Socket.CloseAsync(received.CloseStatus.Value, "exception occured", CancellationToken.None);
                        meta.Socket.Dispose();
                        meta.TokenSource.Cancel();
                    }
                    taskWritingToFs.Dispose();
                }
                else
                {
                    // Socket was not accepted open ...
                     // not (meta.Socket != null && meta.Socket.State == WebSocketState.Open)
                    if (meta.Socket != null)
                    {
                        _logger.LogError($"meta.Socket.State not Open: {meta.Socket.State.ToString()} ");
                        meta.Socket.Dispose();
                    }
                    else
                        _logger.LogError("socket object is null");
                }
                
                
                RemoveLiveInfo(uname);
            }
            catch (IOException ex)
            {
                if (ex.Message == "Unexpected end of stream")
                {
                    _logger.LogError($"Unexpected end of stream");
                }
                else
                {
                    _logger.LogError($"Really unexpected end of stream");
                    await meta.Socket?.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, ex.Message, CancellationToken.None);
                 }
                 meta.Socket?.Dispose();
                
                RemoveLiveInfo(uname);
            }
            return true;
        }
        void RemoveLiveInfo(string userName)
        {
            LiveCastHandler caster;
            if (Casters.TryRemove(userName, out caster)) 
                _logger.LogInformation("removed live info");
            else 
                _logger.LogError("could not remove live info");

        }
    }
}