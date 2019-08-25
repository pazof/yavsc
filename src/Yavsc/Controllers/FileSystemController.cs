using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Yavsc.Helpers;

namespace Yavsc.Controllers
{
    public class FileSystemController : Controller
    {
        ILogger _logger;
        public FileSystemController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FileSystemController>();
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