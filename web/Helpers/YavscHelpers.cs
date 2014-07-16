using System;
using System.Web;
using System.Configuration;

namespace Yavsc.Helpers
{
	public static class YavscHelpers
	{
		private static string siteName = null; 
		public static string SiteName {
			get {
				if (siteName == null) 
					siteName = ConfigurationManager.AppSettings ["Name"];
				return siteName;
			}
		}
	}
}

