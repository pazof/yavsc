using System;
using System.IO;
using System.Linq;
using Yavsc.Abstract.FileSystem;

namespace Yavsc.ViewModels.UserFiles
{
    public class UserDirectoryInfo
    {
        public string UserName { get; set; }
        public string SubPath { get; set; }
        public RemoteFileInfo [] Files {
            get; set;
        }
        public DirectoryShortInfo [] SubDirectories { 
            get; set;
        }
        private DirectoryInfo dInfo;

        // for deserialization
        public UserDirectoryInfo()
        {

        }
        public UserDirectoryInfo(string userReposPath, string username, string path)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new NotSupportedException("No user name, no user dir.");
            UserName = username;
            var finalPath =  username;
            if (!string.IsNullOrWhiteSpace(path))
                 finalPath += Path.DirectorySeparatorChar + path;
            if (!finalPath.IsValidYavscPath())
                throw new InvalidOperationException(
                    $"File name contains invalid chars, using path {finalPath}");

            dInfo = new DirectoryInfo(
                userReposPath+FileSystemConstants.RemoteDirectorySeparator+finalPath);
            if (!dInfo.Exists) dInfo.Create();
            Files = dInfo.GetFiles().Select
             ( entry => new RemoteFileInfo { Name = entry.Name, Size = entry.Length,
             CreationTime = entry.CreationTime, LastModified = entry.LastWriteTime  }).ToArray();
             SubDirectories = dInfo.GetDirectories().Select
             ( d=> new DirectoryShortInfo { Name= d.Name, IsEmpty=false } ).ToArray();
        }
    }

    public class DirectoryShortInfo: IDirectoryShortInfo {
        public string Name { get; set; }
        public bool IsEmpty { get; set; }
    }
}
