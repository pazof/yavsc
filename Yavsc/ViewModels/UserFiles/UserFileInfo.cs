using System;

namespace Yavsc.ViewModels
{
    public class UserFileInfo 
    {

        public string Name { get; set; }

        public long Size { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastModified { get; set; }
    }

}