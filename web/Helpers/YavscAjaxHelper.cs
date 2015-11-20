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

		public static string JString(this AjaxHelper helper, object str)
		{
			return QuoteJavascriptString (str);
		}
	}
}
