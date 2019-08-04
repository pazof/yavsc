using System;
using System.Linq;
using System.Security.Principal;
using System.Security.Claims;
using Yavsc.Models;

namespace Yavsc.Services
{
    public class FileSystemAuthManager : IFileSystemAuthManager
    {
        ApplicationDbContext _dbContext;

        public FileSystemAuthManager(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public FileAccessRight GetFilePathAccess(ClaimsPrincipal user, string normalizedFullPath)
        {
            // Assert (normalizedFullPath!=null)
            var parts = normalizedFullPath.Split('/');
            if (parts.Length<2) return FileAccessRight.None;
            var funame = parts[0];
            if (funame == user.GetUserName()) return FileAccessRight.Read | FileAccessRight.Write;

            var ucl = user.Claims.Where(c => c.Type == YavscClaimTypes.CircleMembership).Select(c => long.Parse(c.Value)).ToArray();

            if (_dbContext.CircleAuthorizationToFile.Any(
                r => r.FullPath == normalizedFullPath && ucl.Contains(r.CircleId)
            )) return FileAccessRight.Read;
            return FileAccessRight.None;
        }

        public string NormalizePath(string path)
        {
            throw new NotImplementedException();
        }

        public void SetAccess(long circleId, string normalizedFullPath, FileAccessRight access)
        {
            throw new NotImplementedException();
        }
    }
}