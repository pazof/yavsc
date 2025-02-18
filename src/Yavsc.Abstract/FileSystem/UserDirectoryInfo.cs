using System;
using System.IO;
using System.Linq;
using Yavsc.Abstract.FileSystem;
using Yavsc.Server.Helpers;

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
        private readonly DirectoryInfo dInfo;

        // for deserialization
        public UserDirectoryInfo()
        {

        }
        
        public UserDirectoryInfo(string userReposPath, string userId, string path)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new NotSupportedException("No user name, no user dir.");
            UserName = userId;
            var finalPath = path == null ? userId : Path.Combine(userId, path);
            if (!finalPath.IsValidYavscPath())
                throw new InvalidOperationException(
                    $"File name contains invalid chars ({finalPath})");

            dInfo = new DirectoryInfo(
                userReposPath+Path.DirectorySeparatorChar+finalPath);
            if (dInfo.Exists) {

                Files = dInfo.GetFiles().Select
                    ( entry => new RemoteFileInfo { Name = entry.Name, Size = entry.Length,
                    CreationTime = entry.CreationTime, LastModified = entry.LastWriteTime  }).ToArray();
                SubDirectories = dInfo.GetDirectories().Select
                    ( d=> new DirectoryShortInfo { Name= d.Name, IsEmpty=false } ).ToArray();
            }
            else {
                // don't return null, but empty arrays
                Files = new RemoteFileInfo[0];
                SubDirectories = new DirectoryShortInfo[0];
            }
        }
    }
}
