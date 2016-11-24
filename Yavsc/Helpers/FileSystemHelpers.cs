
using System.IO;
using System.Linq;
using System.Security.Claims;
using Yavsc.ViewModels.UserFiles;

namespace Yavsc.Helpers
{
    public static class FileSystemHelpers {

        public static UserDirectoryInfo GetUserFiles(this ClaimsPrincipal user,string subdir) {
            
            UserDirectoryInfo di = new UserDirectoryInfo(user.Identity.Name,subdir);
           
            return di;
        }
        static char [] ValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_~.".ToCharArray();

        public static bool IsValidDirectoryName(this string name)
        { 
            return !name.Any(c=> !ValidChars.Contains(c));
        }
        public static bool IsValidPath(this string path)
        { 
            if (path==null) return true;
            foreach (var name in path.Split(Path.DirectorySeparatorChar))
                {
                    if (name!=null)
                    if (!IsValidDirectoryName(name)
                     || name.Equals("..")) return false;
                }
            return true;
        }
    }

}
