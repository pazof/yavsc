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
	public abstract class WebFileSystemManager
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
			ValidateSubDir (destDir,true);
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
			}
		}
		/// <summary>
		/// Checks the sub dir name against model specifications, 
		/// concerning the allowed character class.
		/// </summary>
		/// <param name="subdir">Subdir.</param>
		/// <param name="throwex">throw ex.</param>
		public bool ValidateSubDir (string subdir, bool throwex = false)
		{
			foreach (string dirname in subdir.Split(Path.DirectorySeparatorChar)) {
				if (!Regex.Match (dirname, regexFileName).Success) {
					if (throwex)
						throw new InvalidDirNameException (dirname);
					else
						return false;
				}
				foreach (char x in dirname)
					if (subdir.Contains (x)) {
						if (throwex)
							throw new InvalidDirNameException (subdir);
						else
							return false;
					}
			}
			return true;
		}
		/// <summary>
		/// Gets the files owned by the current logged user.
		/// The web user must be authenticated,
		/// The given username must be registered.
		/// </summary>
		/// <returns>The files.</returns>
		/// <param name="subdir">Subdir.</param>
		public virtual IEnumerable<FileInfo> GetFiles (string subdir)
		{
			string path = Prefix;
			if (subdir != null) {
				ValidateSubDir (subdir,true); // checks for specification validity
				path = Path.Combine (Prefix, subdir);
			}
			DirectoryInfo di = new DirectoryInfo (path);
			return (di.GetFiles ());
		}

		/// <summary>
		/// Files the info.
		/// </summary>
		/// <returns>The info.</returns>
		/// <param name="filePath">file Path.</param>
		public FileInfo FileInfo(string filePath)
		{
			ValidateSubDir (filePath,true);
			return new FileInfo(Path.Combine (Prefix, filePath));
		}
	}
}

