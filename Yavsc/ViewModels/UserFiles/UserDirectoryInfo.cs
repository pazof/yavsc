using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yavsc.Helpers;

namespace Yavsc.ViewModels.UserFiles
{
    public class UserDirectoryInfo
    {
        public string SubPath { get; private set; }
        public UserFileInfo [] Files {
            get; private set;
        }
        public string [] SubDirectories { 
            get; private set;
        }
        private DirectoryInfo dInfo;
        public UserDirectoryInfo(string username, string path)
        {
            SubPath = (path==null) ? username : username +  Path.DirectorySeparatorChar + path;
            if ( !SubPath.IsValidPath() )
                throw new InvalidOperationException(
                    $"File name contains invalid chars, using path {SubPath}");
            
            dInfo = new DirectoryInfo(
                Path.Combine(Startup.UserFilesDirName,SubPath));
            Files = dInfo.GetFiles().Select
             ( entry => new UserFileInfo { Name = entry.Name, Size = entry.Length, 
             CreationTime = entry.CreationTime, LastModified = entry.LastWriteTime  }).ToArray();
             SubDirectories = dInfo.GetDirectories().Select 
             ( d=> d.Name ).ToArray();
        }
    }
}