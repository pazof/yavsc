

using System.Security.Claims;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.FileProviders;
using Yavsc.Models;
using Yavsc.Models.FileSystem;
using Yavsc.Models.Streaming;
using Yavsc.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Http;
using Yavsc.Exceptions;
using Yavsc.Helpers;
using Yavsc.Abstract.Helpers;
namespace Yavsc.Server.Helpers
{
    public static class FileSystemHelpers 
    {
        public static async Task SaveAsAsync(this IFormFile formFile, string path)
        {
            if (formFile.Length > 0) {
                using (Stream fileStream = new FileStream(path, FileMode.Create)) {
                    await formFile.CopyToAsync(fileStream);
                }
            }
        }
        public static FileReceivedInfo ReceiveProSignature(this ClaimsPrincipal user, string billingCode, long estimateId, IFormFile formFile, string signType)
        {
            var item = new FileReceivedInfo(
                Config.SiteSetup.Bills, 
                AbstractFileSystemHelpers.SignFileNameFormat("pro", billingCode, estimateId));

            var fi = new FileInfo(item.FullName);
            if (fi.Exists) item.Overridden = true;

            using (var org = formFile.OpenReadStream())
            {
                using Image image = Image.Load(org);
                image.Save(fi.FullName);
            }
            return item;
        }

        public static string GetAvatarUri(this ApplicationUser user)
        {
            return $"/{Config.SiteSetup.Avatars}/{user.UserName}.png";
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

        /// <summary>
        /// Deletes user file.
        /// User info is modified, but not save in db.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="fileName"></param>

        public static void DeleteUserFile(this ApplicationUser user, string fileName)
        {
            var root = Path.Combine(AbstractFileSystemHelpers.UserFilesDirName, user.UserName);
            var fi = new FileInfo(Path.Combine(root, fileName));
            if (!fi.Exists) return ;
            fi.Delete();
            user.DiskUsage -= fi.Length;
        }
        
        public static FsOperationInfo DeleteUserDirOrFile(this ApplicationUser user, string dirName)
        {
            var root = Path.Combine(AbstractFileSystemHelpers.UserFilesDirName, user.UserName);
            if (string.IsNullOrEmpty(dirName)) 
                return new FsOperationInfo { Done = false, ErrorCode = ErrorCode.InvalidRequest, ErrorMessage = "specify a directory or file name"} ;

            var di = new DirectoryInfo(Path.Combine(root, dirName));
            if (!di.Exists) {
                var fi = new FileInfo(Path.Combine(root, dirName));
                if (!fi.Exists) return new FsOperationInfo { Done = false, ErrorCode = ErrorCode.NotFound, ErrorMessage = "non existent"} ;
                fi.Delete();
                user.DiskUsage -= fi.Length;
            }
            else {
                if (di.GetDirectories().Length>0 || di.GetFiles().Length>0) 
                    return new FsOperationInfo { Done = false, ErrorCode = ErrorCode.InvalidRequest, ErrorMessage = "dir is not empty, refusing to remove it"} ;
                di.Delete();
            }
            return new FsOperationInfo { Done = true };
        }
        
        public static FsOperationInfo MoveUserDir(this ApplicationUser user, string fromDirName, string toDirName)
        {
            var root = Path.Combine(AbstractFileSystemHelpers.UserFilesDirName, user.UserName);
            if (string.IsNullOrEmpty(fromDirName)) 
                return new FsOperationInfo { Done = false, ErrorCode = ErrorCode.InvalidRequest , ErrorMessage = "specify a dir name "} ;

            var di = new DirectoryInfo(Path.Combine(root, fromDirName));
            if (!di.Exists) return new FsOperationInfo { Done = false, ErrorCode = ErrorCode.NotFound,  ErrorMessage = "fromDirName: non existent"} ;
            

            if (string.IsNullOrEmpty(toDirName)) toDirName = ".";
            var destPath = Path.Combine(root, toDirName);


            var fout = new FileInfo(destPath);
            if (fout.Exists) return new FsOperationInfo { Done = false, ErrorCode = ErrorCode.InvalidRequest, ErrorMessage = "destination is a regular file" } ;


            var dout = new DirectoryInfo(destPath);
            if (dout.Exists) {
                destPath = Path.Combine(destPath, dout.Name);
            }
            di.MoveTo(destPath); 
            return new FsOperationInfo { Done = true };
        }
        public static FsOperationInfo MoveUserFileToDir(this ApplicationUser user, string fileNameFrom, string fileNameDest)
        {
            var root = Path.Combine(AbstractFileSystemHelpers.UserFilesDirName, user.UserName);
            var fi = new FileInfo(Path.Combine(root, fileNameFrom));
            if (!fi.Exists) return new FsOperationInfo { ErrorCode = ErrorCode.NotFound, ErrorMessage = "no file to move" } ;
            string dest;
            if (!string.IsNullOrEmpty(fileNameDest)) dest = Path.Combine(root, fileNameDest);
            else dest = root;
            var fo = new FileInfo(dest);
            if (fo.Exists) return new FsOperationInfo { ErrorCode = ErrorCode.DestExists , ErrorMessage = "destination file name is an existing file" } ;
            var dout = new DirectoryInfo(dest);
            if (!dout.Exists) dout.Create();
            fi.MoveTo(Path.Combine(dout.FullName, fi.Name));
            return new FsOperationInfo { Done = true };
        }
        public static FsOperationInfo MoveUserFile(this ApplicationUser user, string fileNameFrom, string fileNameDest)
        {
            var root = Path.Combine(AbstractFileSystemHelpers.UserFilesDirName, user.UserName);
            var fi = new FileInfo(Path.Combine(root, fileNameFrom));
            if (!fi.Exists) return new FsOperationInfo { ErrorCode = ErrorCode.NotFound, ErrorMessage = "no file to move" } ;
            var fo = new FileInfo(Path.Combine(root, fileNameDest));
            if (fo.Exists) return new FsOperationInfo { ErrorCode = ErrorCode.DestExists , ErrorMessage = "destination file name is an existing file" } ;
            fi.MoveTo(fo.FullName);
            return new FsOperationInfo { Done = true };
        }

        static string ParseFileNameFromDisposition(string disposition)
        {
            // form-data_ name=_file__ filename=_Constants.Private.cs_
            var parts = disposition.Split(' ');
            var filename = parts[2].Split('=')[1];
            filename = filename.Substring(1,filename.Length-2);
            return filename;
        }

        public static void AddQuota(this ApplicationUser user, int quota)
        {
            user.DiskQuota += quota;
        }
        public static FileReceivedInfo ReceiveUserFile(this ApplicationUser user, string root, IFormFile f, string destFileName = null)
        {
            return ReceiveUserFile(user, root, f.OpenReadStream(), destFileName ?? ParseFileNameFromDisposition(f.ContentDisposition), f.ContentType, CancellationToken.None);
        }
        
        public static FileReceivedInfo ReceiveUserFile(this ApplicationUser user, string root, Stream inputStream, string destFileName, string contentType, CancellationToken token)
        {
            // TODO lock user's disk usage for this scope,
            // this process is not safe at concurrent access.
            long usage = user.DiskUsage;

            var item = new FileReceivedInfo
            (root, AbstractFileSystemHelpers.FilterFileName(destFileName));
            var fi = new FileInfo(Path.Combine(root, item.FileName));
            if (fi.Exists)
            {
                item.Overridden = true;
                usage -= fi.Length;
            } 
            using (var dest = fi.OpenWrite())
            {
                using (inputStream)
                {
                    const int blen = 1024;
                    byte[] buffer = new byte[blen];
                    int len = 0;
                    while (!token.IsCancellationRequested && (len=inputStream.Read(buffer, 0, blen))>0)
                    {
                        dest.Write(buffer, 0, len);
                        usage += len;
                        if (usage >= user.DiskQuota) break;
                    }
                    user.DiskUsage = usage;
                    dest.Close();
                    inputStream.Close();
                }
            }
            if (usage >= user.DiskQuota) {
                item.QuotaOffense = true;
            }
            user.DiskUsage = usage;
            return item;
        }

        public static HtmlString FileLink(this RemoteFileInfo info, string username, string subpath)
        {
            return new HtmlString( 
                $"{Config.UserFilesOptions.RequestPath}/{username}/{subpath}/{info.Name}" );
        }

        public static RemoteFileInfo FileInfo(this ApplicationUser user, string path)
        {
            IFileInfo info = Config.UserFilesOptions.FileProvider.GetFileInfo($"{user.UserName}/{path}");
            if (!info.Exists) return null;
            return new RemoteFileInfo{ Name = info.Name, Size = info.Length, LastModified = info.LastModified.UtcDateTime };

        }

        public static FileReceivedInfo ReceiveAvatar(this ApplicationUser user, IFormFile formFile)
        {
            var item = new FileReceivedInfo
            (Config.AvatarsOptions.RequestPath.ToUriComponent(),
               user.UserName + ".png");

            using (var org = formFile.OpenReadStream())
            {
                using Image image = Image.Load(org);
                image.Mutate(x=>x.Resize(128,128));
                image.Save(Path.Combine(Config.SiteSetup.Avatars,item.FileName));
                image.Mutate(x=>x.Resize(64,64));
                image.Save(Path.Combine(Config.SiteSetup.Avatars,user.UserName + ".s.png"));
                image.Mutate(x=>x.Resize(32,32));
                image.Save(Path.Combine(Config.SiteSetup.Avatars,user.UserName + ".xs.png"));
            }
            user.Avatar = $"{item.DestDir}/{item.FileName}";
            return item;
        }

        public static string GetFileUrl (this LiveFlow flow)
        {
            // no server-side backup for this stream
            return $"{Config.UserFilesOptions.RequestPath}/{flow.Owner.UserName}/live/"+GetFileName(flow);
        }
        
        public static string GetFileName (this LiveFlow flow)
        {
            var fileInfo = new FileInfo(flow.DifferedFileName);
            var ext = fileInfo.Extension;
            var namelen = flow.DifferedFileName.Length - ext.Length;
            var basename =  flow.DifferedFileName.Substring(0,namelen);
            return $"{basename}-{flow.SequenceNumber}{ext}";
        }

    }
}
