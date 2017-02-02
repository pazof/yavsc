

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Web;
using Microsoft.AspNet.Http;
using Yavsc.Models;
using Yavsc.Models.FileSystem;
using Yavsc.ViewModels;
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
        static char[] ValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_~. ".ToCharArray();

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

        public static FileRecievedInfo ReceiveUserFile(this ApplicationUser user, string root, IFormFile f)
        {
            long usage = user.DiskUsage;

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
                    if (len > (user.DiskQuota - usage)) {
                        
                        return item;
                    }
                    usage += len;

                    while (len > 0)
                    {
                        int blen = len > 1024 ? 1024 : (int)len;
                        org.Read(buffer, 0, blen);
                        dest.Write(buffer, 0, blen);
                        len -= blen;
                    }
                    dest.Close();
                    org.Close();
                    item.Success=true;
                }
            }
            user.DiskUsage = usage;
            return item;
        }
        public static HtmlString FileLink(this DefaultFileInfo info, string username, string subpath)
        {
            return new HtmlString( Startup.UserFilesOptions.RequestPath+"/"+ username + "/" + subpath + "/" +
             HttpUtility.UrlEncode(info.Name) );
        }
        public static FileRecievedInfo ReceiveAvatar(this ApplicationUser user, IFormFile formFile)
        {
            var item = new FileRecievedInfo();
            item.FileName = user.UserName + ".png";
            
            var destFileName = Path.Combine(Startup.SiteSetup.UserFiles.Avatars, item.FileName);

            var fi = new FileInfo(destFileName);
            if (fi.Exists) item.Overriden = true;
            Rectangle cropRect = new Rectangle();

            using (var org = formFile.OpenReadStream())
            {
                Image i = Image.FromStream(org);
                using (Bitmap source = new Bitmap(i))
                {
                    if (i.Width != i.Height)
                    {
                        if (i.Width > i.Height)
                        {
                            cropRect.X = (i.Width - i.Height) / 2;
                            cropRect.Y = 0;
                            cropRect.Width = i.Height;
                            cropRect.Height = i.Height;
                        }
                        else
                        {
                            cropRect.X = 0;
                            cropRect.Y = (i.Height - i.Width) / 2;
                            cropRect.Width = i.Width;
                            cropRect.Height = i.Width;
                        }
                        using (var cropped = source.Clone(cropRect, source.PixelFormat))
                        {
                            CreateAvatars(user,cropped);
                        }
                    }

                }

            }
            item.DestDir = Startup.AvatarsOptions.RequestPath.ToUriComponent();
            user.Avatar = $"{item.DestDir}/{item.FileName}";
            item.Success = true;
            return item;
        }

        private static void CreateAvatars(this ApplicationUser user, Bitmap source)
        {
            var dir = Startup.SiteSetup.UserFiles.Avatars;
            var name = user.UserName + ".png";
            var smallname = user.UserName + ".s.png";
            var xsmallname = user.UserName + ".xs.png";
            using (Bitmap newBMP = new Bitmap(source, 128, 128))
            {
                newBMP.Save(Path.Combine(
                    dir, name), ImageFormat.Png);
            }
            using (Bitmap newBMP = new Bitmap(source, 64, 64))
            {
                newBMP.Save(Path.Combine(
                    dir, smallname), ImageFormat.Png);
            }
            using (Bitmap newBMP = new Bitmap(source, 32, 32))
            {
                newBMP.Save(Path.Combine(
                    dir, xsmallname), ImageFormat.Png);
            }
        }
        public static Func<string,long,string>
          SignFileNameFormat = new Func<string,long,string> ((signType,estimateId) => $"estimate-{signType}sign-{estimateId}.png");

        public static FileRecievedInfo ReceiveProSignature(this ClaimsPrincipal user, long estimateId, IFormFile formFile, string signtype)
        { 
            var item = new FileRecievedInfo();
            item.FileName = SignFileNameFormat("pro",estimateId);
            var destFileName = Path.Combine(Startup.SiteSetup.UserFiles.Bills, item.FileName);

            var fi = new FileInfo(destFileName);
            if (fi.Exists) item.Overriden = true;

            using (var org = formFile.OpenReadStream())
            {
                Image i = Image.FromStream(org);
                using (Bitmap source = new Bitmap(i))
                {
                    source.Save(destFileName, ImageFormat.Png);
                }
            }
            item.Success=true;
            return item;
        }
    }
}
