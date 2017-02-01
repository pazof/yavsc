using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Yavsc.Helpers;
using Yavsc.Models;

namespace Yavsc.ViewComponents
{
    public class AvatarViewComponent : ViewComponent
    {
        ApplicationDbContext dbContext;
        public AvatarViewComponent(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync ( string userId, string imgFmt )
        {
            return View ( "Default", dbContext.AvatarUri(userId, imgFmt));
        }
        
    }
}