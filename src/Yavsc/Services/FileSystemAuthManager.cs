using System;
using System.Linq;
using System.Security.Principal;
using System.Security.Claims;
using Yavsc.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using System.IO;
using rules;
using Microsoft.Data.Entity;

namespace Yavsc.Services
{
    public class FileSystemAuthManager : IFileSystemAuthManager
    {
        class BelongsToCircle : UserMatch
        {
            public override bool Match(string userId)
            {
                return true;
            }
        }
        class OutOfCircle : UserMatch
        {
            public override bool Match(string userId)
            {
                return false;
            }
        }
        UserMatch Out = new OutOfCircle();
        UserMatch In = new BelongsToCircle();

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
            var fileName = parts[parts.Length - 1];


            var firstFileNamePart = parts[3];
            if (firstFileNamePart == "pub" && aclfileName != fileName)
            {
                _logger.LogInformation("Serving public file.");
                return FileAccessRight.Read;
            }
            if (user == null) return FileAccessRight.None;

            var funame = parts[2];
            var cusername = user.GetUserName();
            if (funame == cusername)
            {
                _logger.LogInformation("Serving file to owner.");
                return FileAccessRight.Read | FileAccessRight.Write;
            }
            if (aclfileName == fileName) 
                return FileAccessRight.None;
                
            _logger.LogInformation($"Access to {normalizedFullPath} for {cusername}");

            ruleSetParser.Reset();
            var cuserid = user.GetUserId();
            var fuserid = _dbContext.Users.Single(u => u.UserName == funame).Id;
            var circles = _dbContext.Circle.Include(mb => mb.Members).Where(c => c.OwnerId == fuserid).ToArray();
            foreach (var circle in circles)
            {
                if (circle.Members.Any(m => m.MemberId == cuserid))
                    ruleSetParser.Definitions.Add(circle.Name, In);
                else ruleSetParser.Definitions.Add(circle.Name, Out);
            }

            // _dbContext.Circle.Select(c => c.OwnerId == )
            for (int dirlevel = parts.Length - 1; dirlevel>0; dirlevel--)
            {
                var aclfi = new FileInfo(Path.Combine(Environment.CurrentDirectory, fileDir, aclfileName));
                if (!aclfi.Exists) continue;
                ruleSetParser.ParseFile(aclfi.FullName);
            }
            // TODO default user scoped file access policy
           
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
