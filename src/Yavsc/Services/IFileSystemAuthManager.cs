
using System.Security.Claims;
using Microsoft.Extensions.FileProviders;

namespace Yavsc.Services
{
    [Flags]
    public enum FileAccessRight {
        None = 0,

        Read = 1,
        Write = 2
    }

    public interface IFileSystemAuthManager {
        string NormalizePath (string path);

        /// <summary>
        /// A full path starts with a slash,
        /// continues with a user name,
        /// and returns true by the helper fonction : 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="normalizedFullPath"></param>
        /// <returns></returns>
        FileAccessRight GetFilePathAccess(ClaimsPrincipal user, IFileInfo file);

        void SetAccess (long circleId, string normalizedFullPath, FileAccessRight access);

    }
}
