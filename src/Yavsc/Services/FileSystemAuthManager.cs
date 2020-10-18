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
using Microsoft.AspNet.FileProviders;

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
            ruleSetParser = new RuleSetParser(false);
        }

        public FileAccessRight GetFilePathAccess(ClaimsPrincipal user, IFileInfo file)
        {
            var parts = file.PhysicalPath.Split(Path.DirectorySeparatorChar);
            var cwd = Environment.CurrentDirectory.Split(Path.DirectorySeparatorChar).Length;

            
            // below 3 parts behind cwd, no file name.
            if (parts.Length < cwd + 3) return FileAccessRight.None;

            var fileDir = string.Join("/", parts.Take(parts.Length - 1));
            var fileName = parts[parts.Length - 1];

            var cusername = user.GetUserName();

            var funame = parts[cwd+1];
            if (funame == cusername)
            {
                return FileAccessRight.Read | FileAccessRight.Write;
            }

            if (aclfileName == fileName)
                return FileAccessRight.None;

            ruleSetParser.Reset();
            var cuserid = user.GetUserId();

            var fuserid = _dbContext.Users.SingleOrDefault(u => u.UserName == funame).Id;

            if (string.IsNullOrEmpty(fuserid)) return FileAccessRight.None;

            var circles = _dbContext.Circle.Include(mb => mb.Members).Where(c => c.OwnerId == fuserid).ToArray();
            foreach (var circle in circles)
            {
                if (circle.Members.Any(m => m.MemberId == cuserid))
                    ruleSetParser.Definitions.Add(circle.Name, In);
                else ruleSetParser.Definitions.Add(circle.Name, Out);
            }

            for (int dirlevel = parts.Length - 1; dirlevel > cwd + 1; dirlevel--)
            {
                fileDir = string.Join(Path.DirectorySeparatorChar.ToString(), parts.Take(dirlevel));
                var aclfin = Path.Combine(fileDir, aclfileName);
                var aclfi = new FileInfo(aclfin);
                if (!aclfi.Exists) continue;
                ruleSetParser.ParseFile(aclfi.FullName);
            }

            if (ruleSetParser.Rules.Allow(cusername))
            {
                return FileAccessRight.Read;
            }
            return  FileAccessRight.None;
            // TODO default user scoped file access policy

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
