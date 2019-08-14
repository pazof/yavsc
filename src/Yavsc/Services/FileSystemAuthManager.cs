using System;
using System.Linq;
using System.Security.Principal;
using System.Security.Claims;
using Yavsc.Models;
using Microsoft.Extensions.Logging;

namespace Yavsc.Services
{
    public class FileSystemAuthManager : IFileSystemAuthManager
    {
        ApplicationDbContext _dbContext;
        ILogger _logger;

        public FileSystemAuthManager(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<FileSystemAuthManager>();
        }

        public FileAccessRight GetFilePathAccess(ClaimsPrincipal user, string normalizedFullPath)
        {

            // Assert (normalizedFullPath!=null)
            var parts = normalizedFullPath.Split('/');

            if (parts.Length<4) return FileAccessRight.None;
            var funame = parts[2];
            var filePath = string.Join("/",parts.Skip(3));

            _logger.LogInformation($"{normalizedFullPath} from {funame}");

            if (funame == user?.GetUserName()) return FileAccessRight.Read | FileAccessRight.Write;

            var ucl = user.Claims.Where(c => c.Type == YavscClaimTypes.CircleMembership).Select(c => long.Parse(c.Value)).Distinct().ToArray();
            
            var uclString = string.Join(",", ucl);
            _logger.LogInformation($"{uclString} ");
            foreach (
                var cid in ucl
            ) {
                var ok = _dbContext.CircleAuthorizationToFile.Any(a => a.CircleId == cid && a.FullPath == filePath);
                if (ok) return FileAccessRight.Read;
            }
            
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