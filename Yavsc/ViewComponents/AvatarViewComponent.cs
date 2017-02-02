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

        public IViewComponentResult Invoke ( string userId, string imgFmt )
        {
            return View ( "Default", dbContext.AvatarUri(userId, imgFmt));
        }
        
    }
}