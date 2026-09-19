using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.ViewModels.UserFiles;

namespace Yavsc.ViewComponents
{

    public class DirectoryViewComponent : ViewComponent
    {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly SiteSettings siteSettings;

        public DirectoryViewComponent(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<SiteSettings> siteSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.siteSettings = siteSettings.Value;
        }

        public async Task<IViewComponentResult> InvokeAsync(string dirname)
        {
            string uid = ViewContext.HttpContext.User.GetUserId();
            return View(new UserDirectoryInfo(
                siteSettings.Blog, 
                uid, dirname));
        }
    }
}
