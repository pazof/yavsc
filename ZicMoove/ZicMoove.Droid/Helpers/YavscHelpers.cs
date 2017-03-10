using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using ZicMoove.Model.Auth.Account;
using System.IO;

namespace ZicMoove.Droid
{

	public static class YavscHelpers
	{
		
        public static string GetSpecialFolder(this string specialPath)
        {
            if (specialPath == null)
                throw new InvalidOperationException();
            var appData =
 System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string imagesFolder = System.IO.Path.Combine(appData, specialPath);
            DirectoryInfo di = new DirectoryInfo(imagesFolder);
            // FIXME Create this folder at app startup
            if (!di.Exists) di.Create();
            return imagesFolder;
        }

        public static string GetTmpDir ()
        {
            return GetSpecialFolder("tmp");
        }
	}
}

