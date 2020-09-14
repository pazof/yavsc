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

    public class LiveProcessor : ILiveProcessor
    {
        readonly IHubContext _hubContext;
        private readonly ILogger _logger;
        readonly ApplicationDbContext _dbContext;
        public PathString LiveCastingPath { get; set; } = Constants.LivePath;


        public ConcurrentDictionary<string, LiveCastHandler> Casters { get; } = new ConcurrentDictionary<string, LiveCastHandler>();

        public LiveProcessor(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            _logger = loggerFactory.CreateLogger<LiveProcessor>();
        }

        public async Task<bool> AcceptStream(HttpContext context)
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
            _logger.LogInformation("flow : " + flow.Title + " for " + uname);


            LiveCastHandler liveHandler = null;
            if (Casters.ContainsKey(uname))
            {
                _logger.LogWarning($"Casters.ContainsKey({uname})");
                liveHandler = Casters[uname];
                if (liveHandler.Socket.State == WebSocketState.Open || liveHandler.Socket.State == WebSocketState.Connecting)
                {
                    _logger.LogWarning($"Closing cx");
                    // FIXME loosed connexion should be detected & disposed else where
                    await liveHandler.Socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "one by user", CancellationToken.None);

                }
                if (!liveHandler.TokenSource.IsCancellationRequested)
                {
                    liveHandler.TokenSource.Cancel();
                }
                liveHandler.Socket.Dispose();
                liveHandler.Socket = await context.WebSockets.AcceptWebSocketAsync();
                liveHandler.TokenSource = new CancellationTokenSource();
            }
            else
            {
                _logger.LogInformation($"new caster");
                // Accept the socket
                liveHandler = new LiveCastHandler { Socket = await context.WebSockets.AcceptWebSocketAsync() };
            }
            _logger.LogInformation("Accepted web socket");
            // Dispatch the flow

            try
            {
                if (liveHandler.Socket != null && liveHandler.Socket.State == WebSocketState.Open)
                {
                    Casters[uname] = liveHandler;
                    // TODO: Handle the socket here.
                    // Find receivers: others in the chat room
                    // send them the flow
                    var buffer = new byte[Constants.WebSocketsMaxBufLen];
                    var sBuffer = new ArraySegment<byte>(buffer);
                    _logger.LogInformation("Receiving bytes...");

                    WebSocketReceiveResult received = await liveHandler.Socket.ReceiveAsync(sBuffer, liveHandler.TokenSource.Token);

                    _logger.LogInformation($"Received bytes : {received.Count}");
                    _logger.LogInformation($"Is the end : {received.EndOfMessage}");
                    const string livePath = "live";

                    string destDir = context.User.InitPostToFileSystem(livePath);
                    _logger.LogInformation($"Saving flow to {destDir}");

                    string fileName = flow.GetFileName();
                    FileInfo destFileInfo = new FileInfo(Path.Combine(destDir, fileName));
                    // this should end :-)
                    while (destFileInfo.Exists)
                    {
                        flow.SequenceNumber++;
                        fileName = flow.GetFileName();
                        destFileInfo = new FileInfo(Path.Combine(destDir, fileName));
                    }
                    var fsInputQueue = new Queue<ArraySegment<byte>>();

                    bool endOfInput = false;
                    fsInputQueue.Enqueue(sBuffer);
                    var taskWritingToFs = liveHandler.ReceiveUserFile(user, _logger, destDir, fsInputQueue, fileName, flow.MediaType, () => endOfInput);
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
                        do
                        {

                            _logger.LogInformation($"Echoing {received.Count} bytes received in a {received.MessageType} message; Fin={received.EndOfMessage}");
                            // Echo anything we receive
                            // and send to all listner found
                             _logger.LogInformation($"{liveHandler.Listeners.Count} listeners");
                            foreach (var cliItem in liveHandler.Listeners)
                            {
                                var listenningSocket = cliItem.Value;
                                if (listenningSocket.State == WebSocketState.Open)
                                {
                                    _logger.LogInformation(cliItem.Key);
                                    await listenningSocket.SendAsync(
                                    sBuffer, received.MessageType, received.EndOfMessage, liveHandler.TokenSource.Token);
                                }
                                else if (listenningSocket.State == WebSocketState.CloseReceived || listenningSocket.State == WebSocketState.CloseSent)
                                {
                                    ToClose.Push(cliItem.Key);
                                }
                            }

                            if (!received.CloseStatus.HasValue) 
                            {
                             _logger.LogInformation("try and receive new bytes");

                            buffer = new byte[Constants.WebSocketsMaxBufLen];
                            sBuffer = new ArraySegment<byte>(buffer);
                            received = await liveHandler.Socket.ReceiveAsync(sBuffer, liveHandler.TokenSource.Token);
                            
                            _logger.LogInformation($"Received bytes : {received.Count}");
                            _logger.LogInformation($"segment : offset: {sBuffer.Offset} count: {sBuffer.Count}");
                            _logger.LogInformation($"Is the end : {received.EndOfMessage}");
                            fsInputQueue.Enqueue(sBuffer);
                            if (received.CloseStatus.HasValue)
                            {
                                endOfInput=true;
                                _logger.LogInformation($"received a close status: {received.CloseStatus.Value}: {received.CloseStatusDescription}");
                            }
                            }
                            else endOfInput=true;
                            while (ToClose.Count > 0)
                            {
                                string no = ToClose.Pop();
                                _logger.LogInformation("Closing follower connection");
                                WebSocket listenningSocket;
                                if (liveHandler.Listeners.TryRemove(no, out listenningSocket))
                                {
                                    await listenningSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable,
                                     "State != WebSocketState.Open", CancellationToken.None);
                                    listenningSocket.Dispose();
                                }
                            }
                        }
                        while (liveHandler.Socket.State == WebSocketState.Open);
                        
                        _logger.LogInformation("Closing connection");
                        taskWritingToFs.Wait();
                        await liveHandler.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, received.CloseStatusDescription, liveHandler.TokenSource.Token);

                        liveHandler.TokenSource.Cancel();
                        liveHandler.Dispose();
                        _logger.LogInformation("Resulting file : " + JsonConvert.SerializeObject(taskWritingToFs.Result));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception occured : {ex.Message}");
                        _logger.LogError(ex.StackTrace);
                        liveHandler.TokenSource.Cancel();
                        throw;
                    }
                    taskWritingToFs.Dispose();
                }
                else
                {
                    // Socket was not accepted open ...
                    // not (meta.Socket != null && meta.Socket.State == WebSocketState.Open)
                    if (liveHandler.Socket != null)
                    {
                        _logger.LogError($"meta.Socket.State not Open: {liveHandler.Socket.State} ");
                        liveHandler.Socket.Dispose();
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
                    await liveHandler.Socket?.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, ex.Message, CancellationToken.None);
                }
                liveHandler.Socket?.Dispose();
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
