//
//  FileSystemManager.cs
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
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using System.Text;
using System.Web.Security;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Yavsc.Model.FileSystem
{

	/// <summary>
	/// File system manager.
	/// It performs the FileSystem controllers logic.
	/// It will not be a true file system,
	/// It just provides simple method for a small set of 
	/// files, in a small tree of sub-folders .
	/// </summary>
	public class FileSystemManager
	{
		/// <summary>
		/// Gets or sets the size of the max file.
		/// </summary>
		/// <value>The size of the max file.</value>
		public long MaxFileSize { get; set; }

		/// <summary>
		/// Gets or sets the max user storage.
		/// </summary>
		/// <value>The max user storage.</value>
		public long MaxUserStorage { get; set; }


		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FileSystem.FileSystemManager"/> class.
		/// </summary>
		public FileSystemManager (string usersDirectory="~/files", char dirSep = '/')
		{
			prefix = usersDirectory;
			DirectorySeparator = dirSep;

		}

		string regexFileName = "^[A-Za-z0-9#^!+ _~\\-.]+$";

		/// <summary>
		/// Put the specified files in destDir, as sub dir of the current user's home dir.
		/// </summary>
		/// <param name="destDir">Destination dir, use "." to point to the user's home dir.</param>
		/// <param name="files">Files.</param>
		public void Put (string destDir, NameObjectCollectionBase files)
		{
			// sanity check on file names
			foreach (object obj in files) {
				HttpPostedFileBase file = obj as HttpPostedFileBase;
				if (!Regex.Match (file.FileName, regexFileName).Success) {
					throw new InvalidOperationException (string.Format (
						"The file name {0} dosn't match an acceptable file name ({1})",
						file.FileName, regexFileName));
				}
			}
			// do the job

			checkSubDir (destDir);
			DirectoryInfo di = new DirectoryInfo (
				Path.Combine (Prefix, destDir));
			if (!di.Exists)
				di.Create ();

			foreach (object obj in files) {
				HttpPostedFileBase file = obj as HttpPostedFileBase;
				// TODO Limit with hfc[h].ContentLength
				string filename = Path.Combine (di.FullName, file.FileName);
				file.SaveAs (filename);
			}
		}

		private string prefix = null;

		/// <summary>
		/// Gets the users dir.
		/// </summary>
		/// <value>The users dir.</value>
		public string Prefix {
			get {
				return prefix;
			}
			set {
				prefix = value;
				if (value.StartsWith ("~/")) {
					prefix = HttpContext.Current.Server.MapPath(value);
				}
			}
		}

		/// <summary>
		/// Gets or sets the directory name separator.
		/// </summary>
		/// <value>The separator.</value>
		public char DirectorySeparator { get; set; }

		/// <summary>
		/// Checks the sub dir name.
		/// </summary>
		/// <param name="subdir">Subdir.</param>
		private void checkSubDir (string subdir)
		{
			foreach (string dirname in subdir.Split(DirectorySeparator))
				if (!Regex.Match (dirname, regexFileName).Success)
					throw new InvalidDirNameException (dirname);
			foreach (char x in Path.GetInvalidPathChars()) 
				if (subdir.Contains (x)) 
					throw new InvalidDirNameException (subdir);
		}
		/// <summary>
		/// Gets the files.
		/// The web user must be authenticated,
		/// The given username must be registered.
		/// </summary>
		/// <returns>The files.</returns>
		/// <param name="subdir">Subdir.</param>
		public FileInfoCollection GetFiles (string subdir)
		{
			string path = Prefix;
			if (subdir != null) {
				checkSubDir (subdir);
				path = Path.Combine (Prefix, subdir);
			}
			DirectoryInfo di = new DirectoryInfo (path);
			FileInfoCollection res = new FileInfoCollection (di.GetFiles ());
			return res;
		}
	}
}

