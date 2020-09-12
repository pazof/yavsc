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
        readonly ApplicationDbContext _dbContext;
        readonly ILogger _logger;

        public FileSystemAuthManager(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<FileSystemAuthManager>();
        }

        public FileAccessRight GetFilePathAccess(ClaimsPrincipal user, string normalizedFullPath)
        {

            // Assert (normalizedFullPath!=null)
            var parts = normalizedFullPath.Split('/');

            // below 4 parts, no file name.
            if (parts.Length<4) return FileAccessRight.None;
            
            var filePath = string.Join("/",parts.Skip(3));

            var firstFileNamePart = parts[3];
            if (firstFileNamePart == "pub") 
                {
                    _logger.LogInformation("Serving public file.");
                    return FileAccessRight.Read;
                }

            var funame = parts[2];
            _logger.LogInformation($"{normalizedFullPath} from {funame}");
           
            if (funame == user?.GetUserName()) 
                {
                    _logger.LogInformation("Serving file to owner.");
                    return FileAccessRight.Read | FileAccessRight.Write;
                }


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
