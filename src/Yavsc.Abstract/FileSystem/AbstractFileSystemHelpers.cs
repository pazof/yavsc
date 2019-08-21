using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Yavsc.ViewModels.UserFiles;

using System;
using System.Threading;
using Yavsc.ViewModels;

namespace Yavsc.Helpers
{
    public static class AbstractFileSystemHelpers
    {
        public static string UserBillsDirName {  set; get; }
        public static string UserFilesDirName {  set; get; }
        
        public static bool IsValidYavscPath(this string path)
        {
            if (string.IsNullOrEmpty(path)) return true;
            foreach (var name in path.Split(Path.DirectorySeparatorChar))
            {
                if (!IsValidDirectoryName(name) || name.Equals("..") || name.Equals("."))
                        return false;
            }
            if (path[path.Length-1]==RemoteDirectorySeparator) return false;
            return true;
        }
        public static bool IsValidDirectoryName(this string name)
        {
            return !name.Any(c => !ValidFileNameChars.Contains(c));
        }
        // Ensure this path is canonical,
        // No "dirto/./this", neither "dirt/to/that/"
        // no .. and each char must be listed as valid in constants

        public static string FilterFileName(string fileName)
        {
            if (fileName==null) return null;
            StringBuilder sb = new StringBuilder();
            foreach (var c in fileName)
            {
                if (ValidFileNameChars.Contains(c))
                    sb.Append(c);
                else sb.Append('_');
            }
           return sb.ToString();
        }
  
        public static UserDirectoryInfo GetUserFiles(string userName, string subdir)
        {

            UserDirectoryInfo di = new UserDirectoryInfo(UserFilesDirName, userName, subdir);
            return di;
        }
        public const char RemoteDirectorySeparator = '/';
        public static char[] ValidFileNameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-=_~. ".ToCharArray();
   

        public static Func<string,string,long,string>
          SignFileNameFormat = new Func<string,string,long,string> ((signType,billingCode,estimateId) => $"sign-{billingCode}-{signType}-{estimateId}.png");


        
    }
}