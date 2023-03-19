using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Yavsc.Helpers;

namespace Yavsc.Controllers
{
    public class FileSystemController : Controller
    {
        public FileSystemController()
        {
        }

        public IActionResult Index(string subdir="")
        { 
            if (subdir !=null)
                if (!subdir.IsValidYavscPath())
                    return new BadRequestResult();
            var files = AbstractFileSystemHelpers.GetUserFiles(User.Identity.Name, subdir);
            return View(files);
        }
    }
}
