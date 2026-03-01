using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using rules;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Server.Helpers;

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

        /// <summary>
        /// Get the file access info, for a given user, to a given FS absolute path
        /// </summary>
        /// <param name="clientUser"></param>
        /// <param name="providerUserFilePath"></param>
        /// <returns></returns>
        public FileAccessRight GetFilePathAccess(ClaimsPrincipal clientUser, string providerUserFilePath)
        {

            var clientUserName = clientUser.GetUserName();
            FileInfo fi = new FileInfo(
                Path.Combine(Config.UserFilesDirName, providerUserFilePath));

            if (providerUserFilePath.StartsWith(clientUserName + '/'))
            {
                return FileAccessRight.Read | FileAccessRight.Write;
            }
            var providerUserName = providerUserFilePath.Split('/')[0];

            ruleSetParser.Reset();
            var cuserid = clientUser.GetUserId();

            var providerUserId = _dbContext.Users.SingleOrDefault(u => u.UserName == providerUserName).Id;

            if (string.IsNullOrEmpty(providerUserId)) return FileAccessRight.None;

            var circles = _dbContext.Circle.Include(mb => mb.Members).Where(c => c.OwnerId == providerUserId).ToArray();
            foreach (var circle in circles)
            {
                if (circle.Members.Any(m => m.MemberId == cuserid))
                    ruleSetParser.Definitions.Add(circle.Name, In);
                else ruleSetParser.Definitions.Add(circle.Name, Out);
            }
            var userFilesDir = new DirectoryInfo(
                Path.Combine(Config.UserFilesDirName, providerUserName));
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


            if (ruleSetParser.Rules.Allow(clientUserName))
            {
                return FileAccessRight.Read;
            }
            return FileAccessRight.None;
            // TODO default user scoped file access policy

        }

        public void SetAccess(long circleId, string normalizedFullPath, FileAccessRight access)
        {
            throw new NotImplementedException();
        }
    }
}
