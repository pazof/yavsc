//
//  YavscAjaxHelper.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015 GNU GPL
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Web.Mvc;
using System.Collections.Generic;
using Yavsc.Model.Messaging;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace Yavsc.Helpers
{
	/// <summary>
	/// Yavsc ajax helper.
	/// </summary>
	public static class YavscAjaxHelper
	{
		/// <summary>
		/// Notify the specified helper, message and click_action.
		/// </summary>
		/// <param name="helper">Helper.</param>
		/// <param name="message">Message.</param>
		/// <param name="click_action">Click action.</param>
		public static void Notify(this AjaxHelper helper, object message, string click_action=null) {
			
			if (helper.ViewData ["Notifications"] == null)
				helper.ViewData ["Notifications"] = new List<Notification> ();
			(helper.ViewData ["Notifications"] as List<Notification>).Add (
				new Notification { body = QuoteJavascriptString((string)message), 
					click_action = click_action } ) ;
		}
		/// <summary>
		/// Globalizations the script.
		/// </summary>
		/// <returns>The script.</returns>
		/// <param name="ajaxHelper">Ajax helper.</param>
		public static MvcHtmlString YaGlobalizationScript(this AjaxHelper ajaxHelper)
		{
			return YaGlobalizationScript(ajaxHelper, CultureInfo.CurrentCulture);
		}
		/// <summary>
		/// Globalizations the script.
		/// </summary>
		/// <returns>The script.</returns>
		/// <param name="ajaxHelper">Ajax helper.</param>
		/// <param name="cultureInfo">Culture info.</param>
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ajaxHelper", Justification = "This is an extension method")]
		public static MvcHtmlString YaGlobalizationScript(this AjaxHelper ajaxHelper, CultureInfo cultureInfo)
		{
			return YaGlobalizationScriptHelper(AjaxHelper.GlobalizationScriptPath, cultureInfo);
		}

		internal static MvcHtmlString YaGlobalizationScriptHelper(string scriptPath, CultureInfo cultureInfo)
		{
			if (cultureInfo == null)
			{
				throw new ArgumentNullException("cultureInfo");
			}

			// references the global script :
			TagBuilder tagBuilder = new TagBuilder("script");
			tagBuilder.MergeAttribute("type", "text/javascript");
			string src = VirtualPathUtility.AppendTrailingSlash(scriptPath) + "globalize.js";
			tagBuilder.MergeAttribute("src", VirtualPathUtility.ToAbsolute (src));

			TagBuilder tagCurrentBuilder = new TagBuilder("script");
			string srccurrent = VirtualPathUtility.AppendTrailingSlash(scriptPath) + "cultures/globalize.culture." + HttpUtility.UrlEncode(cultureInfo.Name) + ".js";
			tagCurrentBuilder.MergeAttribute("src", VirtualPathUtility.ToAbsolute (srccurrent));

			string html = tagBuilder.ToString(TagRenderMode.Normal)+tagCurrentBuilder.ToString(TagRenderMode.Normal);
			return new MvcHtmlString(html);
		}
		/// <summary>
		/// Quotes the javascript string.
		/// </summary>
		/// <returns>The javascript string.</returns>
		/// <param name="str">String.</param>
		public static string QuoteJavascriptString(object str)
		{
			string tmpstr = (string) str;
			tmpstr = tmpstr.Replace ("\n", "\\n");
			if (tmpstr.Contains ("'"))
			if (tmpstr.Contains ("\""))
				return "'" + tmpstr.Replace ("'", "\\'") + "'";
			else
				return "\"" + tmpstr + "\"";
			return "'" + tmpstr + "'";
		}
		/// <summary>
		/// Js the son string.
		/// </summary>
		/// <returns>The son string.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="obj">Object.</param>
		public static string JSonString(this AjaxHelper helper, object obj)
		{
			string result = null;
			DataContractJsonSerializer ser = new DataContractJsonSerializer (obj.GetType());
			var e = Encoding.UTF8;
			using (MemoryStream streamQuery = new MemoryStream ()) {
				ser.WriteObject (streamQuery, obj);
				streamQuery.Seek (0, SeekOrigin.Begin);
				using (StreamReader sr = new StreamReader (streamQuery)) {
					result = sr.ReadToEnd ();
				}
			}
			return result;
		}

		/// <summary>
		/// Js the string.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="text">Text.</param>
		public static string JString(this AjaxHelper helper, object text)
		{
			return QuoteJavascriptString ((string)text);
		}
	}
}
