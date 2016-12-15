using Microsoft.AspNet.FileProviders;

namespace Yavsc.ViewModels.Auth
{
    public class ViewFileContext
    {
        public string UserName { get; set; }
        public IFileInfo File { get; set; }
        public string Path { get; set; }
    }
}