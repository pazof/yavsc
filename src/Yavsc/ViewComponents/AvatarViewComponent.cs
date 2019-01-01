using System.Linq;
using Microsoft.AspNet.Mvc;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.ViewModels.Account;

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
            var user = dbContext.Users.Single(u=>u.Id == userId);

            return View ( "Default", new ShortUserInfo
            {
                Avatar = dbContext.AvatarUri(userId, imgFmt),
                UserName = user.UserName,
                UserId = userId
            } );
        }
        
    }
}