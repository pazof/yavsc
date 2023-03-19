using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Yavsc.Helpers;
using Yavsc.ViewModels.UserFiles;

namespace Yavsc.ViewComponents
{

    public class DirectoryViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string dirname)
        {
            IViewComponentResult result = null;
            await Task.Run(() =>
            {
                result = View(new UserDirectoryInfo(AbstractFileSystemHelpers.UserFilesDirName, User.Identity.Name, dirname));
            });
            return result;
        }
    }
}
