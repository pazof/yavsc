using System.IO;
using Microsoft.AspNet.FileProviders;
using Yavsc.Models.Billing;

namespace Yavsc.Helpers
{
    public static class FileSystemHelpers {
      public static IDirectoryContents GetFileContent(this Estimate estimate, string userFileDir)
        {
            if (estimate?.Query?.PerformerProfile?.Performer == null)
                return null;
            var di = new DirectoryInfo(Path.Combine(
                    userFileDir,
                    estimate.Query.PerformerProfile.Performer.UserName
               ));
            if (!di.Exists) return null;

            var fsp = new PhysicalFileProvider(di.FullName);
            return fsp.GetDirectoryContents(
                Path.Combine(Constants.UserBillsFilesDir, estimate.Id.ToString())
                );
        }
    }

}
