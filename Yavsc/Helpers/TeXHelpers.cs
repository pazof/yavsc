using System.Linq;

namespace Yavsc.Helpers
{
    public static class TeXHelpers
    {
        public static string NewLinesWith(this string target, string separator)
        { 
            var items = target.Split(new char[] {'\n'}).Where(
                s=> !string.IsNullOrWhiteSpace(s) ) ;

            return string.Join(separator, items);
        }
    }
}