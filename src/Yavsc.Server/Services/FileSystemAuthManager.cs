using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using rules;
using Yavsc.Helpers;
using Yavsc.Models;

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

        private readonly UserMatch Out = new OutOfCircle();
        private readonly UserMatch In = new BelongsToCircle();

        private readonly ApplicationDbContext _dbContext;

        private readonly SiteSettings SiteSettings;

        readonly RuleSetParser ruleSetParser;

        public FileSystemAuthManager(ApplicationDbContext dbContext, IOptions<SiteSettings> sitesOptions)
        {
            _dbContext = dbContext;
            SiteSettings = sitesOptions.Value;
            ruleSetParser = new RuleSetParser(false);
        }

        public FileAccessRight GetFilePathAccess(ClaimsPrincipal user, string fileRelativePath)
        {
           
            var cusername = user.GetUserName();
            FileInfo fi = new FileInfo(
                Path.Combine(Config.UserFilesDirName, fileRelativePath));
           
            if (fileRelativePath.StartsWith(cusername+'/'))
            {
                return FileAccessRight.Read | FileAccessRight.Write;
            }
            var funame = fileRelativePath.Split('/')[0];

            // TODO Assert valid user name

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
            var userFilesDir = new DirectoryInfo(
                Path.Combine(Config.UserFilesDirName, funame));
            var currentACLDir = fi.Directory;
            do
            {
                var aclfileName = Path.Combine(currentACLDir.FullName, 
                SiteSettings.AccessListFileName);
                FileInfo accessFileInfo = new FileInfo(aclfileName);
                if (accessFileInfo.Exists)
                    ruleSetParser.ParseFile(accessFileInfo.FullName);
                currentACLDir = currentACLDir.Parent;
            } while (currentACLDir != userFilesDir);


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
