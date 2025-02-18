
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Attributes.Validation;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Messaging;
using Yavsc.Services;
using Microsoft.AspNetCore.SignalR;
using Yavsc.Server.Helpers;

namespace Yavsc.ApiControllers
{
    [Authorize, Route("api/stream")]
    public partial class FileSystemStreamController : Controller
    {
        private readonly ILogger logger;
        private readonly ILiveProcessor liveProcessor;
        private readonly IHubContext<ChatHub> hubContext;
        readonly ApplicationDbContext dbContext;

        public FileSystemStreamController(ApplicationDbContext context, ILiveProcessor liveProcessor, ILoggerFactory loggerFactory,
        IHubContext<ChatHub> hubContext)
        {
            this.dbContext = context;
            this.logger = loggerFactory.CreateLogger<FileSystemStreamController>();
            this.liveProcessor = liveProcessor;
            this.hubContext = hubContext;
        }

        [Authorize, Route("put/{filename}")]
        public async Task<IActionResult> Put([ValidRemoteUserFilePath] string filename)
        {
            logger.LogInformation("Put : " + filename);
            if (!HttpContext.WebSockets.IsWebSocketRequest)
                return BadRequest("not a web socket");
            if (!HttpContext.User.Identity.IsAuthenticated)
                return new UnauthorizedResult();
            var subdirs = filename.Split('/');
            var filePath = subdirs.Length > 1 ? string.Join("/", subdirs.Take(subdirs.Length-1)) : null;
            var shortFileName = subdirs[subdirs.Length-1];
            if (!shortFileName.IsValidShortFileName())
            {
                logger.LogInformation("invalid file name : " + filename);
                return BadRequest("invalid file name");
            }
            logger.LogInformation("validated: api/stream/Put: "+filename);
            var userName = User.GetUserName();
            
            string url = string.Format(
                "{0}/{1}/{2}",
                Config.UserFilesOptions.RequestPath.ToUriComponent(),
                userName,
                filename
            );

            hubContext.Clients.All.SendAsync("addPublicStream", new PublicStreamInfo
            {
                sender = userName,
                url = url,
            }, $"{userName} is starting a stream!");
            
            string destDir = HttpContext.User.InitPostToFileSystem(filePath);
            logger.LogInformation($"Saving flow to {destDir}");
            var userId = User.GetUserId();
            var user = await dbContext.Users.FirstAsync(u => u.Id == userId);
            logger.LogInformation("Accepting stream ...");
            await liveProcessor.AcceptStream(HttpContext, user, destDir, shortFileName);
            return Ok();
        }
    }
}
