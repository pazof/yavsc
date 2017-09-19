using System.Linq;
using Yavsc.Models;

namespace Yavsc.Helpers
{
    public static class UserHelpers
    {
        /// <summary>
        /// The avatar ...
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="userId"></param>
        /// <param name="imgFmt"></param>
        /// <returns></returns>
        // FIXME support imgFmt
        public static string AvatarUri(this ApplicationDbContext dbContext, string userId, string imgFmt )
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user==null) return Constants.AnonAvatar;
            if (user.Avatar==null) return Constants.DefaultAvatar;
            var avatar = user.UserName;
            return $"/Avatars/{avatar}{imgFmt}.png";
        }
    }
}
