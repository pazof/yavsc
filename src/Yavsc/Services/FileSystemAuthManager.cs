using System;
using System.Linq;
using System.Security.Principal;
using System.Security.Claims;
using Yavsc.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using System.IO;
using rules;

namespace Yavsc.Services
{
    public class FileSystemAuthManager : IFileSystemAuthManager
    {
        readonly ApplicationDbContext _dbContext;
        readonly ILogger _logger;

        readonly SiteSettings SiteSettings;

        readonly string aclfileName;

        readonly RuleSetParser ruleSetParser;

        public FileSystemAuthManager(ApplicationDbContext dbContext, ILoggerFactory loggerFactory,
            IOptions<SiteSettings> sitesOptions)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<FileSystemAuthManager>();
            SiteSettings = sitesOptions.Value;
            aclfileName = SiteSettings.AccessListFileName;
            ruleSetParser = new RuleSetParser(true);
        }

        public FileAccessRight GetFilePathAccess(ClaimsPrincipal user, string normalizedFullPath)
        {

            // Assert (normalizedFullPath!=null)
            var parts = normalizedFullPath.Split('/');

            // below 4 parts, no file name.
            if (parts.Length < 4) return FileAccessRight.None;

            var fileDir = string.Join("/", parts.Take(parts.Length - 1));

            var firstFileNamePart = parts[3];
            if (firstFileNamePart == "pub")
            {
                _logger.LogInformation("Serving public file.");
                return FileAccessRight.Read;
            }

            var funame = parts[2];
            _logger.LogInformation($"Accessing {normalizedFullPath} from {funame}");

            if (funame == user?.GetUserName())
            {
                _logger.LogInformation("Serving file to owner.");
                return FileAccessRight.Read | FileAccessRight.Write;
            }
            var aclfi = new FileInfo(Path.Combine(Environment.CurrentDirectory, fileDir, aclfileName));
            // TODO default user scoped file access policy
            if (!aclfi.Exists) return FileAccessRight.Read;
            ruleSetParser.Reset();
            ruleSetParser.ParseFile(aclfi.FullName);
            if (ruleSetParser.Rules.Allow(user.GetUserName()))
                return FileAccessRight.Read;

            var ucl = user.Claims.Where(c => c.Type == YavscClaimTypes.CircleMembership).Select(c => long.Parse(c.Value)).Distinct().ToArray();

            var uclString = string.Join(",", ucl);
            _logger.LogInformation($"{uclString} ");
            foreach (
                var cid in ucl
            )
            {
                var ok = _dbContext.CircleAuthorizationToFile.Any(a => a.CircleId == cid && a.FullPath == fileDir);
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
