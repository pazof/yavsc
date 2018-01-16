using System.Linq;

namespace Yavsc.Abstract.FileSystem
{
    public static class FileSystemHelpers
    {
        public static bool IsValidYavscPath(this string path)
        {
            if (path == null) return true;
            foreach (var name in path.Split('/'))
            {
                if (!IsValidDirectoryName(name) || name.Equals("..") || name.Equals("."))
                        return false;
            }
            if (path[path.Length]==FileSystemConstants.RemoteDirectorySeparator) return false;
            return true;
        }
        public static bool IsValidDirectoryName(this string name)
        {
            return !name.Any(c => !FileSystemConstants.ValidFileNameChars.Contains(c));
        }

    }

    public static  class FileSystemConstants
    {
        public const char RemoteDirectorySeparator = '/';
        public static char[] ValidFileNameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-=_~. ".ToCharArray();
    }
}