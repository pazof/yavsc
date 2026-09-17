using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Yavsc.Server.Helpers;

namespace Yavsc.Org.Controllers
{
    public class FileSystemController : Controller
    {
        private SiteSettings _siteSettings;

        public FileSystemController(IOptions<SiteSettings> siteSettings)
        {
            _siteSettings = siteSettings.Value;
        }
        // Removed redundant empty constructor block

        public IActionResult Index(string subdir="")
        {
            if (subdir !=null)
                if (!subdir.IsValidYavscPath())
                    return new BadRequestResult();
            var files = FileSystemHelpers.GetUserFiles(_siteSettings, User.GetUserId(), subdir);
            return View(files);
        }
    }
}
