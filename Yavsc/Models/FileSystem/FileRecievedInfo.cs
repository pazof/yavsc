namespace Yavsc.Models.FileSystem
{
        public class FileRecievedInfo
        {
            public bool Success { get; set; }
            public string DestDir { get; set; }
            public string FileName { get; set; }
            public bool Overriden { get; set; }
        }
}