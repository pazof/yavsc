using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Yavsc.Helpers;
using Yavsc.Models;

namespace Yavsc.ApiControllers
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Yavsc.Abstract.FileSystem;
    using Yavsc.Exceptions;
    using Yavsc.Models.FileSystem;

    public class FSQuotaException : Exception {

    }

    [Authorize,Route("api/fs")]
    public class FileSystemApiController : Controller
    {
        ApplicationDbContext dbContext;
        private IAuthorizationService AuthorizationService;
        private ILogger logger;

        public FileSystemApiController(ApplicationDbContext context,
        IAuthorizationService authorizationService, 
        ILoggerFactory loggerFactory)

        {
            AuthorizationService = authorizationService;
            dbContext = context;
            logger = loggerFactory.CreateLogger<FileSystemApiController>();
        }

        [HttpGet()]
        public IActionResult Get()
        {
            return GetDir(null);
        }

        [HttpGet("{subdir}")]
        public IActionResult GetDir(string subdir="")
        {
            if (subdir !=null)
                if (!subdir.IsValidYavscPath())
                    return new BadRequestResult();
            var files = User.GetUserFiles(subdir);
            return Ok(files);
        }

        [HttpPost]
        public IActionResult Post(string subdir="", string names = null)
        {
            string root = null;
            string [] destinationFileNames = names?.Split('/');
            List<FileRecievedInfo> received = new List<FileRecievedInfo>();
            InvalidPathException pathex = null;
            try {
                root = User.InitPostToFileSystem(subdir);
            } catch (InvalidPathException ex) {
                pathex = ex;
            }
            if (pathex!=null)
              return new BadRequestObjectResult(pathex);
            var uid = User.GetUserId();
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            int i=0;
            logger.LogInformation($"Recieving {Request.Form.Files.Count} files.");
            foreach (var f in Request.Form.Files)
            {
                var destFileName = destinationFileNames?.Length >i ? destinationFileNames[i] : null;

                var item = user.ReceiveUserFile(root, f, destFileName);
                dbContext.SaveChanges(User.GetUserId());
                received.Add(item);
                logger.LogInformation($"Recieved  '{item.FileName}'.");
                if (item.QuotaOffensed)
                    break;
                i++;
            };
            return Ok(received);
        }

        [HttpDelete]
        public async Task <IActionResult> Delete (string id)
        {
            var user = dbContext.Users.Single(
                u => u.Id == User.GetUserId()
            );
            InvalidPathException pathex = null;
            string root = null;
            try {
                root = User.InitPostToFileSystem(id);
            } catch (InvalidPathException ex) {
                pathex = ex;
            }
            if (pathex!=null)
              return new BadRequestObjectResult(pathex);
            user.DeleteUserFile(id);
            await dbContext.SaveChangesAsync(User.GetUserId());
             return Ok(new { deleted=id });
        }
    }
}
