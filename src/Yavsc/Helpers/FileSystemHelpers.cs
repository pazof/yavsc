

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mime;
using System.Security.Claims;
using System.Web;
using Microsoft.AspNet.Http;
using Yavsc.Abstract.FileSystem;
using Yavsc.Exceptions;
using Yavsc.Models;
using Yavsc.Models.FileSystem;
using Yavsc.ViewModels;

namespace Yavsc.Helpers
{
    public static class FileSystemHelpers
    {
        public static Func<string,string,long,string>
          SignFileNameFormat = new Func<string,string,long,string> ((signType,billingCode,estimateId) => $"sign-{billingCode}-{signType}-{estimateId}.png");


public static FileRecievedInfo ReceiveProSignature(this ClaimsPrincipal user, string billingCode, long estimateId, IFormFile formFile, string signtype)
        {
            var item = new FileRecievedInfo();
            item.FileName = SignFileNameFormat("pro",billingCode,estimateId);
            item.MimeType = formFile.ContentDisposition;
            
            var destFileName = Path.Combine(Startup.SiteSetup.Bills, item.FileName);

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
            return item;
        }

        private static void CreateAvatars(this ApplicationUser user, Bitmap source)
        {
            var dir = Startup.SiteSetup.Avatars;
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

        public static string InitPostToFileSystem(
            this ClaimsPrincipal user,
            string subpath)
        {
            var root = Path.Combine(AbstractFileSystemHelpers.UserFilesDirName, user.Identity.Name);
            var diRoot = new DirectoryInfo(root);
            if (!diRoot.Exists) diRoot.Create();
            if (!string.IsNullOrWhiteSpace(subpath)) {
                if (!subpath.IsValidYavscPath())
                {
                    throw new InvalidPathException();
                }
                root = Path.Combine(root, subpath);
            }
            var di = new DirectoryInfo(root);
            if (!di.Exists) di.Create();
            return di.FullName;
        }

        public static void DeleteUserFile(this ApplicationUser user, string fileName)
        {
            var root = Path.Combine(AbstractFileSystemHelpers.UserFilesDirName, user.UserName);
            var fi = new FileInfo(Path.Combine(root, fileName));
            if (!fi.Exists) return ;
            fi.Delete();
            user.DiskUsage -= fi.Length;
        }
        public static FileRecievedInfo ReceiveUserFile(this ApplicationUser user, string root, IFormFile f, string destFileName = null)
        {
            long usage = user.DiskUsage;

            var item = new FileRecievedInfo();
            // form-data; name="file"; filename="capt0008.jpg"
            ContentDisposition contentDisposition = new ContentDisposition(f.ContentDisposition);
            item.FileName = Yavsc.Abstract.FileSystem.AbstractFileSystemHelpers.FilterFileName (destFileName ?? contentDisposition.FileName);
            item.MimeType = contentDisposition.DispositionType;
            item.DestDir = root;
            var fi = new FileInfo(Path.Combine(root, item.FileName));
            if (fi.Exists)
            {
                item.Overriden = true;
                usage -= fi.Length;
            } 
            using (var dest = fi.OpenWrite())
            {
                using (var org = f.OpenReadStream())
                {
                    byte[] buffer = new byte[1024];
                    long len = org.Length;
                    if (len > (user.DiskQuota - usage)) {
                        item.QuotaOffensed = true;
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
                }
            }
            user.DiskUsage = usage;
            return item;
        }
        public static HtmlString FileLink(this RemoteFileInfo info, string username, string subpath)
        {
            return new HtmlString( Startup.UserFilesOptions.RequestPath+"/"+ username + 
                "/" + (( subpath == null ) ? "" : "/" + subpath ) +
             info.Name );
        }
        public static FileRecievedInfo ReceiveAvatar(this ApplicationUser user, IFormFile formFile)
        {
            var item = new FileRecievedInfo();
            item.FileName = user.UserName + ".png";

            var destFileName = Path.Combine(Startup.SiteSetup.Avatars, item.FileName);

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
            return item;
        }


    }
}