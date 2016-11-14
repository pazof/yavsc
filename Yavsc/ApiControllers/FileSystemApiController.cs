using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Yavsc.Helpers;
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
            public string ContentDisposition { get; set; }
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