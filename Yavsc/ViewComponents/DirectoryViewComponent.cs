using Microsoft.AspNet.Mvc;

namespace Yavsc.ViewComponents
{
    using System.Threading.Tasks;
    using Yavsc.ViewModels.UserFiles;

    public class DirectoryViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string dirname)
        {
            IViewComponentResult result = null;
            await Task.Run(() =>
            {
                result = View(new UserDirectoryInfo(Startup.UserFilesDirName, User.Identity.Name, dirname));
            });
            return result;
        }
    }
}