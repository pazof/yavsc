﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Yavsc.ViewModels.UserFiles;

namespace Yavsc.Server.Helpers
{
    public static class AbstractFileSystemHelpers
    {
        public static string UserBillsDirName {  set; get; }
        public static string UserFilesDirName {  set; get; }

        /// <summary>
        /// Is Valid this Path?
        /// Return true when given value is a valid user file sub-path,
        /// regarding chars used to specify it.
        /// </summary>
        /// <param name="path">Path to validate</param>
        /// <returns></returns>
        public static bool IsValidYavscPath(this string path)
        {
            if (string.IsNullOrEmpty(path)) return true;
            // disallow full path specification
            if (path[0]==Path.DirectorySeparatorChar) return false;
            foreach (var name in path.Split(Path.DirectorySeparatorChar))
            {
                if (!IsValidDirectoryName(name) || name.Equals("..") || name.Equals("."))
                        return false;
            }
            // disallow trailling slash
            if (path[path.Length-1]==RemoteDirectorySeparator) return false;
            return true;
        }

        public static bool IsValidDirectoryName(this string name)
        {
            return !name.Any(c => !ValidFileNameChars.Contains(c));
        }

        public static bool IsValidShortFileName(this string name)
        {
            if (name.Any(c => !ValidFileNameChars.Contains(c)))
                return false;
            
            if (!name.Any(c => !AlfaNum.Contains(c)))
                return false;
                
            return true;
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
                else sb.Append("#" + ((int)c).ToString("D3"));
            }
            return sb.ToString();
        }

        public static UserDirectoryInfo GetUserFiles(string userId, string subdir)
        {
            UserDirectoryInfo di = new UserDirectoryInfo(UserFilesDirName, userId, subdir);
            return di;
        }
        public static bool IsRegularFile(string userName, string subdir)
        {
            FileInfo fi = new FileInfo( Path.Combine(Path.Combine(UserFilesDirName, userName), subdir));
            return fi.Exists;
        }


        // Server side only supports POSIX file systems
        public const char RemoteDirectorySeparator = '/';

        public static char[] AlfaNum = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        // Only accept descent remote file names
        public static char[] ValidFileNameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-=_~. %#@".ToCharArray();

        // Estimate signature file name format
        public static Func<string, string, long, string>
          SignFileNameFormat = new Func<string, string, long, string>((signType, billingCode, estimateId) => $"sign-{billingCode}-{signType}-{estimateId}.png");

    }
}
