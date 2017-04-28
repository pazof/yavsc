using System;
using System.IO;
using System.Linq;
using Yavsc.Helpers;

namespace Yavsc.ViewModels.UserFiles
{
    public class UserDirectoryInfo
    {
        public string UserName { get; private set; }
        public string SubPath { get; private set; }
        public DefaultFileInfo [] Files {
            get; private set;
        }
        public string [] SubDirectories { 
            get; private set;
        }
        private DirectoryInfo dInfo;
        public UserDirectoryInfo(string username, string path)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new NotSupportedException("No user name, no user dir.");
            UserName = username;
            var finalPath =  username;
            if (!string.IsNullOrWhiteSpace(path))
                 finalPath += Path.DirectorySeparatorChar + path;
            if (!finalPath.IsValidPath())
                throw new InvalidOperationException(
                    $"File name contains invalid chars, using path {finalPath}");

            dInfo = new DirectoryInfo(
                Startup.UserFilesDirName+Path.DirectorySeparatorChar+finalPath);
            if (!dInfo.Exists) dInfo.Create();
            Files = dInfo.GetFiles().Select
             ( entry => new DefaultFileInfo { Name = entry.Name, Size = entry.Length,
             CreationTime = entry.CreationTime, LastModified = entry.LastWriteTime  }).ToArray();
             SubDirectories = dInfo.GetDirectories().Select
             ( d=> d.Name ).ToArray();
        }
    }
}
