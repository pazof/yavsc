
using System.Security.Claims;
using Microsoft.Extensions.FileProviders;

namespace Yavsc.Services
{
    [Flags]
    public enum FileAccessRight : Byte {
        None = 0,

        Read = 1,
        Write = 2
    }

    public interface IFileSystemAuthManager {

        /// <summary>
        ///  
        /// </summary>
        /// <param name="user"></param>
        /// <param name="normalizedFullPath"></param>
        /// <returns></returns>
        FileAccessRight GetFilePathAccess(ClaimsPrincipal user, string fileRelativePath);

        void SetAccess (long circleId, string normalizedFullPath, FileAccessRight access);

    }
}
