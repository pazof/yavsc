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
            UserName = username;
            var finalPath = (path==null) ? username : username +  Path.DirectorySeparatorChar + path;
            if ( !finalPath.IsValidPath() )
                throw new InvalidOperationException(
                    $"File name contains invalid chars, using path {SubPath}");
            
            dInfo = new DirectoryInfo(
                Path.Combine(Startup.UserFilesDirName,finalPath));
            Files = dInfo.GetFiles().Select
             ( entry => new DefaultFileInfo { Name = entry.Name, Size = entry.Length, 
             CreationTime = entry.CreationTime, LastModified = entry.LastWriteTime  }).ToArray();
             SubDirectories = dInfo.GetDirectories().Select 
             ( d=> d.Name ).ToArray();
        }
    }
}