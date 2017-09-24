using System.IO;
using System.Reflection;
using System.Resources;

namespace Yavsc.Resources
{

    ///<summary>
    /// Makes possible the code injection
    ///</summary>
    public class YavscLocalisation : ResourceSet
    {
        public YavscLocalisation(string path) : base(path)
        {
        }
        public YavscLocalisation(Stream stream) : base(stream)
        {
        }
        public static string PassAndConfirmDontMach
        {
            get
            {
                return Startup.GlobalLocalizer["PassAndConfirmDontMach"];
            }
        }
        public static string UserName { get { return Startup.GlobalLocalizer["UserName"]; } }
        public static string Password { get { return Startup.GlobalLocalizer["Password"]; } }
        public static string PasswordConfirm { get { return Startup.GlobalLocalizer["PasswordConfirm"]; } }



    }
}
