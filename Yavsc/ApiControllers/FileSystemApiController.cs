using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Yavsc.Models;

namespace Yavsc.ApiControllers
{
    [Authorize,Route("api/fs")]
    public class FileSystemApiController : Controller
    {
        private IAuthorizationService AuthorizationService;
        public FileSystemApiController(ApplicationDbContext context,
        IAuthorizationService authorizationService)

        {
            AuthorizationService = authorizationService;
        }
        
        [HttpGet()]
        public IActionResult Get()
        {
            return GetDir(null);
        }

        [HttpGet("{subdir}")]
        public IActionResult GetDir(string subdir)
        {
            var path = User.GetUserId();
            if (subdir!=null) path = Path.Combine(path,subdir);
            var result = Startup.UserFilesOptions.FileProvider.GetDirectoryContents(path);
            return Ok(result.Select(
                c => new { Name = c.Name, IdDir = c.IsDirectory }
            ));
        }

        public class FileRecievedInfo
        {
            public string DestDir { get; set; }
            public string ContentDisposition { get; set; }
            public bool Overriden { get; set; }
        }
        [HttpPost]
        public IEnumerable<FileRecievedInfo> Post()
        {
            var root = Path.Combine(Startup.UserFilesDirName, User.GetUserId());

            foreach (var f in Request.Form.Files.GetFiles("Files"))
            {
                var item = new FileRecievedInfo();
                item.ContentDisposition = f.ContentDisposition;
                var fi = new FileInfo(Path.Combine(root, f.ContentDisposition));
                if (fi.Exists) item.Overriden = true;
                using (var dest = fi.OpenWrite())
                {
                    using (var org = f.OpenReadStream())
                    {
                        byte[] buffer = new byte[1024];
                        int o = 0, c;
                        while ((c = org.Read(buffer, o, 1024)) > 0)
                        {
                            dest.Write(buffer, o, c);
                            o += 1024;
                            // TODO quota
                        }
                        dest.Close();
                        org.Close();
                    }
                }
                yield return item;
            };
        }
    }
}