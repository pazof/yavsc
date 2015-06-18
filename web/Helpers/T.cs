using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Net.Mail;
using Yavsc;
using System.Globalization;
using Yavsc.Model;

namespace Yavsc.Helpers
{
	/// <summary>
	/// T.
	/// </summary>
	public static class T
	{
		
		/// <summary>
		/// Gets the string.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="msg">Message.</param>
		public static string GetString(string msg)
		{
			string tr = LocalizedText.ResourceManager.GetString (msg.Replace (" ", "_"));
			return tr==null?msg:tr;
		}

		public static string Translate(this HtmlHelper helper, string text)
		{
			// Just call the other one, to avoid having two copies (we don't use the HtmlHelper).
			return GetString(text);
		}

	}
}
