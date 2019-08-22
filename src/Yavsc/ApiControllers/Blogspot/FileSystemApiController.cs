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
    using Yavsc.Helpers;
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

        [HttpGet("{*subdir}")]
        public IActionResult GetDir(string subdir="")
        {
            if (subdir !=null)
                if (!subdir.IsValidYavscPath())
                    return new BadRequestResult();
            var files = AbstractFileSystemHelpers.GetUserFiles(User.Identity.Name, subdir);
            return Ok(files);
        }

        [HttpPost("{*subdir}")]
        public IActionResult Post(string subdir="")
        {
            string destDir = null;
            List<FileRecievedInfo> received = new List<FileRecievedInfo>();
            InvalidPathException pathex = null;
            try {
                destDir = User.InitPostToFileSystem(subdir);
            } catch (InvalidPathException ex) {
                pathex = ex;
            }
            if (pathex!=null) {
                logger.LogError($"invalid sub path: '{subdir}'.");
                return HttpBadRequest(pathex);
            }
            logger.LogInformation($"Receiving files, saved in '{destDir}' (specified as '{subdir}').");
            
            var uid = User.GetUserId();
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            int i=0;
            logger.LogInformation($"Receiving {Request.Form.Files.Count} files.");
            
            foreach (var f in Request.Form.Files)
            {
                var item = user.ReceiveUserFile(destDir, f);
                dbContext.SaveChanges(User.GetUserId());
                received.Add(item);
                logger.LogInformation($"Received  '{item.FileName}'.");
                if (item.QuotaOffensed)
                    break;
                i++;
            };
            return Ok(received);
        }

        [Route("/api/fsquota/add/{uname}/{len}")]
        [Authorize("AdministratorOnly")]
        public IActionResult AddQuota(string uname, int len)
        {
            var uid = User.GetUserId();
            var user = dbContext.Users.Single(
                u => u.UserName == uname
            );
            user.AddQuota(len);
            dbContext.SaveChanges(uid);
            return Ok(len);
        }

        [Route("/api/movefile")]
        [Authorize()]
        public IActionResult MoveFile(string from, string to)
        {
            var uid = User.GetUserId();
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            throw new NotImplementedException();
            return Ok();
        }

        [Route("/api/movedir")]
        [Authorize()]
        public IActionResult MoveDir(string from, string to)
        {
            var uid = User.GetUserId();
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            throw new NotImplementedException();
            return Ok();
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
