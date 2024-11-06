using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.ViewModels.UserFiles;

namespace Yavsc.ViewComponents
{

    public class DirectoryViewComponent : ViewComponent
    {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
        public DirectoryViewComponent(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string dirname)
        {
            string uid = ViewContext.HttpContext.User.GetUserId();

            IViewComponentResult result = null;

            result = View(new UserDirectoryInfo(
                AbstractFileSystemHelpers.UserFilesDirName, 
                uid, dirname));
           
            return result;
        }
    }
}
