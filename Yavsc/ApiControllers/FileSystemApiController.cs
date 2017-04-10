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
    using Yavsc.Exceptions;
    public class FSQuotaException : Exception {

    }

    [Authorize,Route("api/fs")]
    public class FileSystemApiController : Controller
    {
        ApplicationDbContext dbContext;
        private IAuthorizationService AuthorizationService;
        public FileSystemApiController(ApplicationDbContext context,
        IAuthorizationService authorizationService)

        {
            AuthorizationService = authorizationService;
            dbContext = context;
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
                if (!FileSystemHelpers.IsValidPath(subdir))
                    return new BadRequestResult();
            var files = User.GetUserFiles(subdir);
            return Ok(files);
        }


        [HttpPost]
        public IEnumerable<IActionResult> Post(string subdir="")
        {
            string root = null;
            try {
             root = User.InitPostToFileSystem(subdir);
            } catch (InvalidPathException) {}
            if (root==null)
             yield return new BadRequestObjectResult(new { error= "InvalidPathException" });
            var user = dbContext.Users.Single(
                u => u.Id == User.GetUserId()
            );

            foreach (var f in Request.Form.Files)
            {
                var item = user.ReceiveUserFile(root, f);
                dbContext.SaveChanges(User.GetUserId());
                yield return Ok(item);
            };
        }
    }
}
