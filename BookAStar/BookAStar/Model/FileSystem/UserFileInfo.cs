using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar.Model.FileSystem
{
    public class UserFileInfo
    {
        public string Name { get; set; }

        public long Size { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastModified { get; set; }
    }
}
