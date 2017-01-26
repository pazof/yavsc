using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZicMoove.Model.FileSystem
{
    public class FileAddress : IEquatable<FileAddress>
    {
        public string SubPath { get; set; }
        public string UserName { get; set; }

        public bool Equals(FileAddress other)
        {
            if (other == null) return false;
            if (other.UserName != UserName) return false;
            if (other.SubPath != SubPath) return false;
            return true;
        }
    }

    public class UserDirectoryInfo : FileAddress
    {
        public UserFileInfo[] Files
        {
            get;  set;
        }

        public string[] SubDirectories
        {
            get;  set;
        }
    }
}
