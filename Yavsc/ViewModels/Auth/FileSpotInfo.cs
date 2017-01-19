
using System.IO;
using Microsoft.AspNet.Authorization;
using Yavsc.Models;

namespace Yavsc.ViewModel.Auth {

    public class FileSpotInfo : IAuthorizationRequirement
    {
        public DirectoryInfo PathInfo { get; private set; }

        public FileSpotInfo(string path, Blog b) {
            PathInfo = new DirectoryInfo(path);
            AuthorId = b.AuthorId;
            BlogEntryId = b.Id;
        }
        public string AuthorId { get; private set; }
        public long BlogEntryId { get; private set; }
        
    }
    
    
    

}