using Yavsc.Abstract.FileSystem;

namespace Yavsc.ViewModels.UserFiles
{
    public class DirectoryShortInfo: IDirectoryShortInfo {
        public string Name { get; set; }
        public bool IsEmpty { get; set; }
    }
}
