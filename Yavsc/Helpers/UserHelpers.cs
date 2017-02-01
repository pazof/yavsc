
using System.IO;
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
            if (user.Avatar.StartsWith("/"))
                {
                    // use fmt
                    FileInfo fi = new FileInfo(user.Avatar);
                    var ext = fi.Extension;
                    var avatar = user.Avatar.Substring(0,  user.Avatar.Length - ext.Length );
                    return $"{avatar}{imgFmt}{ext}";
                }
            return user.Avatar;
        }
    }
}
