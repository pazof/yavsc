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
		public static void Notify(this AjaxHelper helper, string message, string click_action=null) {
			
			if (helper.ViewData ["Notifications"] == null)
				helper.ViewData ["Notifications"] = new List<Notification> ();
			(helper.ViewData ["Notifications"] as List<Notification>).Add (
				new Notification { body = QuoteJavascriptString(message), 
					click_action = click_action } ) ;
		}

		/// <summary>
		/// Quotes the javascript string.
		/// </summary>
		/// <returns>The javascript string.</returns>
		/// <param name="str">String.</param>
		public static string QuoteJavascriptString(string str)
		{
			str = str.Replace ("\n", "\\n");
			if (str.Contains ("'"))
			if (str.Contains ("\""))
				return "'" + str.Replace ("'", "\\'") + "'";
			else
				return "\"" + str + "\"";
			return "'" + str + "'";
		}
	}
}
