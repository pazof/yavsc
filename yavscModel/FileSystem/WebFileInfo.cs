//
//  ShortFileName.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 Paul Schneider
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
using System.ComponentModel.DataAnnotations;

using System.Web.Mvc;
using System.IO;
using System.Web;

namespace Yavsc.Model.FileSystem
{
	/// <summary>
	/// Files in the name.
	/// </summary>
	public class WebFileInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FileSystem.WebFileInfo"/> class.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="id">Identifier.</param>
		public WebFileInfo(HttpContextBase context, string id) {
			DirectoryInfo di=new DirectoryInfo(
				HttpContext.Current.Server.MapPath(id));

			if (!di.Exists)
				throw new Exception (string.Format(
					"Inexistent:{0}", id));
			path = id;
			permaLink = UrlHelper.GenerateContentUrl(id,context);
		}
		private string path = null;
		string permaLink = null;
		/// <summary>
		/// Gets the perma link.
		/// </summary>
		/// <value>The perma link.</value>
		public string PermaLink {
			get {
				return permaLink;
			}
		}

		/// <summary>
		/// Gets the Path.
		/// </summary>
		/// <value>The web dir.</value>
		public string Path {
			get {
				return path;
			}
		}

		/*
		/// <summary>
		/// Creates the name of the file.
		/// </summary>
		/// <returns>The file name.</returns>
		/// <param name="intentValue">Intent value.</param>
		/// <param name="destdir">Destdir.</param>
		public static FileInfo BuildUniqueFileName(string intentValue, string destdir) {

			int nbTries = 0;
			FileInfo res = new FileInfo (Path.Combine(destdir,intentValue));

			foreach(var property in res.GetType().GetProperties())
			{
				if(property.Name == "Name")
				{
					object [] atts = property.GetCustomAttributes(typeof(RegularExpressionAttribute), true);
					// we only keep the last one
					// ASSERT(atts.Length>0) 
					if (!((RegularExpressionAttribute)(atts [atts.Length - 1])).IsValid (res))
						throw new InvalidFileNameException (intentValue);
				} 
			}

			while (new FileInfo (Path.Combine (di.FullName, res.ShortName)).Exists)
				{
					res.ShortName = intentValue + "-" + nbTries++;
				}
			return res;
		}*/

	}
}

