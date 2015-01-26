using System;
using System.Web;
using System.Configuration;

namespace Yavsc.Helpers
{
	/// <summary>
	/// Yavsc helpers.
	/// </summary>
	public static class YavscHelpers
	{
		private static string siteName = null; 
		/// <summary>
		/// Gets the name of the site.
		/// </summary>
		/// <value>The name of the site.</value>
		public static string SiteName {
			get {
				if (siteName == null) 
					siteName = ConfigurationManager.AppSettings ["Name"];
				return siteName;
			}
		}
	}
}

