using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Yavsc.Models;

namespace Yavsc.ApiControllers
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Yavsc.Helpers;
    using Yavsc.Exceptions;
    using Yavsc.Models.FileSystem;
    using System.ComponentModel.DataAnnotations;
    using Yavsc.Attributes.Validation;

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
        public IActionResult GetDir([ValidRemoteUserFilePath] string subdir="")
        { 
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            // _logger.LogInformation($"listing files from {User.Identity.Name}{subdir}");
            var files = AbstractFileSystemHelpers.GetUserFiles(User.Identity.Name, subdir);
            return Ok(files);
        }

        [HttpPost("{*subdir}")]
        public IActionResult Post([ValidRemoteUserFilePath] string subdir="")
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
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

        [Route("/api/fsc/addquota/{uname}/{len}")]
        [Authorize("AdministratorOnly")]
        public IActionResult AddQuota(string uname, int len)
        {
            var uid = User.GetUserId();
            var user = dbContext.Users.FirstOrDefault(
                u => u.UserName == uname
            );
            if (user==null) return new BadRequestObjectResult(new { error = "no such use" });
            user.AddQuota(len);
            dbContext.SaveChanges(uid);
            return Ok(len);
        }

        [Route("/api/fsc/movefile")]
        [Authorize()]
        public IActionResult MoveFile([ValidRemoteUserFilePath] string from, [ValidRemoteUserFilePath] string to)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            var uid = User.GetUserId();
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            var info = user.MoveUserFile(from, to);
            if (!info.Done)
            return new BadRequestObjectResult(info);
            return Ok();
        }

        [HttpPatch]
        [Route("/api/fsc/movedir")]
        [Authorize()]
        public IActionResult MoveDir([ValidRemoteUserFilePath] string from,[ValidRemoteUserFilePath] string to)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            var uid = User.GetUserId();
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            try {
                var result = user.MoveUserDir(from, to);
                if (!result.Done)
                    return new BadRequestObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(
                    new FsOperationInfo {
                        Done = false,
                        Error = ex.Message
                });
            }
            return Ok();
        }

        [HttpDelete("{*id}")]
        public IActionResult RemoveDirOrFile ([ValidRemoteUserFilePath] string id)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);

            var user = dbContext.Users.Single(
                u => u.Id == User.GetUserId()
            );

            try {
                var result = user.DeleteUserDirOrFile(id);
                if (!result.Done)
                    return new BadRequestObjectResult(result);
            }

            catch (Exception ex)
            {
                return new BadRequestObjectResult(
                    new FsOperationInfo {
                        Done = false,
                        Error = ex.Message
                });
            }
            return Ok(new { deleted=id });
        }
    }

}
