
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNet.Http;
using Yavsc.ApiControllers;
using Yavsc.Models;
using Yavsc.Models.FileSystem;
using Yavsc.ViewModels.UserFiles;

namespace Yavsc.Helpers
{
    public static class FileSystemHelpers
    {

        public static UserDirectoryInfo GetUserFiles(this ClaimsPrincipal user, string subdir)
        {

            UserDirectoryInfo di = new UserDirectoryInfo(user.Identity.Name, subdir);

            return di;
        }
        static char[] ValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_~.".ToCharArray();

        public static bool IsValidDirectoryName(this string name)
        {
            return !name.Any(c => !ValidChars.Contains(c));
        }
        public static bool IsValidPath(this string path)
        {
            if (path == null) return true;
            foreach (var name in path.Split(Path.DirectorySeparatorChar))
            {
                if (name != null)
                    if (!IsValidDirectoryName(name)
                     || name.Equals(".."))
                        return false;
            }
            return true;
        }
        public static string InitPostToFileSystem(
            this ClaimsPrincipal user,
            string subpath)
        {
            var root = Path.Combine(Startup.UserFilesDirName, user.Identity.Name);
            // TOSO secure this path  
            // if (subdir!=null) root = Path.Combine(root, subdir);
            var diRoot = new DirectoryInfo(root);
            if (!diRoot.Exists) diRoot.Create();
            if (subpath != null)
                if (subpath.IsValidPath())
                {
                    root = Path.Combine(root, subpath);
                    diRoot = new DirectoryInfo(root);
                    if (!diRoot.Exists) diRoot.Create();
                }

            return root;
        }

        public static FileRecievedInfo ReceiveUserFile(this ApplicationUser user, string root, long quota, ref long usage, IFormFile f)
        {
            var item = new FileRecievedInfo();
            // form-data; name="file"; filename="capt0008.jpg"
            ContentDisposition contentDisposition = new ContentDisposition(f.ContentDisposition);
            item.FileName = contentDisposition.FileName;
            var fi = new FileInfo(Path.Combine(root, item.FileName));
            if (fi.Exists) item.Overriden = true;
            using (var dest = fi.OpenWrite())
            {
                using (var org = f.OpenReadStream())
                {
                    byte[] buffer = new byte[1024];
                    long len = org.Length;
                    user.DiskUsage += len;
                    if (len > (quota - usage)) throw new FSQuotaException();

                    while (len > 0)
                    {
                        int blen = len > 1024 ? 1024 : (int)len;
                        org.Read(buffer, 0, blen);
                        dest.Write(buffer, 0, blen);
                        len -= blen;
                    }
                    dest.Close();
                    org.Close();
                }
            }
            return item;
        }
    }

}
