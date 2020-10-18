using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Yavsc.Attributes.Validation;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Messaging;
using Yavsc.Services;

namespace Yavsc.ApiControllers
{
    [Authorize, Route("api/stream")]
    public partial class FileSystemStreamController : Controller
    {
        private readonly ILogger logger;
        private readonly ILiveProcessor liveProcessor;
        readonly ApplicationDbContext dbContext;

        public FileSystemStreamController(ApplicationDbContext context, ILiveProcessor liveProcessor, ILoggerFactory loggerFactory)
        {
            this.dbContext = context;
            this.logger = loggerFactory.CreateLogger<FileSystemStreamController>();
            this.liveProcessor = liveProcessor;
        }

        [Authorize, Route("put/{filename}")]
        public async Task<IActionResult> Put([ValidRemoteUserFilePath] string filename)
        {
            logger.LogInformation("Put : " + filename);
            if (!HttpContext.WebSockets.IsWebSocketRequest)
                return HttpBadRequest("not a web socket");
            if (!HttpContext.User.Identity.IsAuthenticated)
                return new HttpUnauthorizedResult();
            var subdirs = filename.Split('/');
            var filePath = subdirs.Length > 1 ? string.Join("/", subdirs.Take(subdirs.Length-1)) : null;
            var shortFileName = subdirs[subdirs.Length-1];
            if (!shortFileName.IsValidShortFileName())
            {
                logger.LogInformation("invalid file name : " + filename);
                return HttpBadRequest("invalid file name");
            }
            logger.LogInformation("validated: api/stream/Put: "+filename);
            var userName = User.GetUserName();
            var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            string url = string.Format(
                "{0}/{1}/{2}",
                Startup.UserFilesOptions.RequestPath.ToUriComponent(),
                userName,
                filename
            );

            hubContext.Clients.All.addPublicStream(new PublicStreamInfo
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
