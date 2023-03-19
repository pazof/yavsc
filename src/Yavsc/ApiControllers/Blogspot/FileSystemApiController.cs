
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    using System.IO;

    [Authorize,Route("api/fs")]
    public partial class FileSystemApiController : Controller
    {
        readonly ApplicationDbContext dbContext;
        private readonly IAuthorizationService AuthorizationService;
        private readonly ILogger _logger;

        public FileSystemApiController(ApplicationDbContext context,
        IAuthorizationService authorizationService, 
        ILoggerFactory loggerFactory)

        {
            AuthorizationService = authorizationService;
            dbContext = context;
            _logger = loggerFactory.CreateLogger<FileSystemApiController>();
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
                _logger.LogError($"invalid sub path: '{subdir}'.");
                return BadRequest(pathex);
            }
            _logger.LogInformation($"Receiving files, saved in '{destDir}' (specified as '{subdir}').");
            
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            int i=0;
            _logger.LogInformation($"Receiving {Request.Form.Files.Count} files.");
            
            foreach (var f in Request.Form.Files)
            {
                var item = user.ReceiveUserFile(destDir, f);
                dbContext.SaveChanges(User.GetUserId());
                received.Add(item);
                _logger.LogInformation($"Received  '{item.FileName}'.");
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
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = dbContext.Users.FirstOrDefault(
                u => u.UserName == uname
            );
            if (user==null) return new BadRequestObjectResult(new { error = "no such use" });
            user.AddQuota(len);
            dbContext.SaveChanges(uid);
            return Ok(len);
        }

        [HttpPost]
        [Route("/api/fsc/mvftd")]
        [Authorize()]
        public IActionResult MoveFile([FromBody] RenameFileQuery query)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            var info = user.MoveUserFileToDir(query.id, query.to);
            if (!info.Done) return new BadRequestObjectResult(info);
            return Ok(new { moved = query.id });
        }

        [HttpPost]
        [Route("/api/fsc/mvf")]
        [Authorize()]
        public IActionResult RenameFile([FromBody] RenameFileQuery query)
        {
            if (!ModelState.IsValid) {
                var idvr = new ValidRemoteUserFilePathAttribute();

                return this.BadRequest(new { id = idvr.IsValid(query.id), to = idvr.IsValid(query.to), errors = ModelState });
            }
            _logger.LogInformation($"Valid move query: {query.id} => {query.to}");
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            try {
                if (Startup.UserFilesOptions.FileProvider.GetFileInfo(Path.Combine(user.UserName, query.id)).Exists)
                {
                    var result = user.MoveUserFile(query.id, query.to);
                    if (!result.Done) return new BadRequestObjectResult(result);
                }
                else {
                    var result = user.MoveUserDir(query.id, query.to);
                    if (!result.Done) return new BadRequestObjectResult(result);
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(
                    new FsOperationInfo {
                        Done = false,
                        ErrorCode = ErrorCode.InternalError,
                        ErrorMessage = ex.Message
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
                        ErrorCode = ErrorCode.InternalError,
                        ErrorMessage = ex.Message
                });
            }
            return Ok(new { deleted=id });
        }

        
    }

}
