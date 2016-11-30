using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Yavsc.Helpers;
using Yavsc.Models;

namespace Yavsc.ApiControllers
{

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

        public class FileRecievedInfo
        {
            public string DestDir { get; set; }
            public string FileName { get; set; }
            public bool Overriden { get; set; }
        }
        [HttpPost]
        public IEnumerable<FileRecievedInfo> Post(string subdir="")
        {
            var root = Path.Combine(Startup.UserFilesDirName, User.Identity.Name);
            // TOSO secure this path  
            // if (subdir!=null) root = Path.Combine(root, subdir);
            var diRoot = new DirectoryInfo(root);
            if (!diRoot.Exists) diRoot.Create();

            var user = dbContext.Users.Single(
                u => u.Id == User.GetUserId()
            );
            var quota = user.DiskQuota;
            var usage = user.DiskUsage;

            foreach (var f in Request.Form.Files)
            {
                var item = new FileRecievedInfo();
                // form-data; name="file"; filename="capt0008.jpg"
                ContentDisposition contentDisposition = new ContentDisposition(f.ContentDisposition);
                item.FileName = contentDisposition.FileName;
                var fi = new FileInfo(Path.Combine(root, item.FileName));
                if (fi.Exists) item.Overriden = true;
                using (var dest = fi.OpenWrite())
                {
                    using (var org = f.OpenReadStream())
                    {
                        byte[] buffer = new byte[1024];
                        long len = org.Length;
                        user.DiskUsage += len;
                        if (len> (quota-usage)) throw new FSQuotaException();

                        while (len>0) {
                            int blen = len>1024?1024:(int)len;
                            org.Read(buffer, 0, blen);
                            dest.Write(buffer,0,blen);
                            len-=blen;
                        }
                        dest.Close();
                        org.Close();
                    }
                }
                dbContext.SaveChanges();
                yield return item;
            };
        }
    }
}