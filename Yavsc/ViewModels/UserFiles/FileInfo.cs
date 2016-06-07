using System;

namespace Yavsc.ViewModels
{
    public class FileInfo
    {

        public string PermanentUri { get; set; }
        public string Name { get; set; }

        public int Size { get; set; }

        public DateTime Creation { get; set; }

        public string MimeType { get; set; }

    }

}