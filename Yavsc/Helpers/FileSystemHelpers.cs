using System.IO;
using Microsoft.AspNet.FileProviders;
using Yavsc.Models;

namespace Yavsc.Helpers
{
    public static class FileSystemHelpers {
      public static IDirectoryContents GetFileContent(this Estimate estimate, string userFileDir)
        {
            if (estimate?.Command?.PerformerProfile?.Performer == null)
                return null;
            var fsp = new PhysicalFileProvider(
                Path.Combine(
                    userFileDir,
                    estimate.Command.PerformerProfile.Performer.UserName
               ));
            return fsp.GetDirectoryContents(
                Path.Combine(Constants.UserBillsFilesDir, estimate.Id.ToString())
                );
        }
    }

}
