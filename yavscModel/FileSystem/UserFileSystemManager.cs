//
//  UserFileSystemManager.cs
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
using System.IO;
using System.Web;
using System.Web.Security;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Yavsc.Model.FileSystem
{
	/// <summary>
	/// User file system.
	/// </summary>
	internal class UserFileSystem  : WebFileSystemManager {
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FileSystem.UserFileSystem"/> class.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="root">Root.</param>
		public UserFileSystem(string username, string root="~/users") {
			base.Prefix = UserFileSystemManager.UserFileRoot(username,root);
		}
	}

	/// <summary>
	/// User file system manager.
	/// </summary>
	public static class UserFileSystemManager 
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FileSystem.UserFileSystemManager"/> class.
		/// </summary>
		/// <param name="rootDirectory">Root directory.</param>
		public static string CurrentUserFileRoot (string rootDirectory="~/users")
		{
			if (!HttpContext.Current.User.Identity.IsAuthenticated)
				throw new Exception ("Not membership available");
			return UserFileRoot (HttpContext.Current.User.Identity.Name,rootDirectory);
		}
		/// <summary>
		/// Users the file root.
		/// </summary>
		/// <returns>The file root.</returns>
		/// <param name="username">Username.</param>
		/// <param name="rootDirectory">Root directory.</param>
		public static string UserFileRoot (string username, string rootDirectory="~/users")
		{
			string rootpath = HttpContext.Current.Server.MapPath (rootDirectory);
			return Path.Combine(rootpath, username);
		}
		private static WebFileSystemManager manager=null;
		/// <summary>
		/// Gets or sets the file manager.
		/// </summary>
		/// <value>The file manager.</value>
		public static WebFileSystemManager FileManager { 
			get
			{ 
				if (manager == null)
					manager = new UserFileSystem (
						HttpContext.Current.User.Identity.Name);
				return manager;
			}
			set { 
				manager = value;
			}
		}
		/// <summary>
		/// Gets the files.
		/// </summary>
		/// <returns>The files.</returns>
		/// <param name="subdir">Subdir.</param>
		public static IEnumerable<FileInfo> GetFiles (string subdir)
		{
			return GetFiles (HttpContext.Current.User.Identity.Name, subdir);
		}
		/// <summary>
		/// Gets the files.
		/// </summary>
		/// <returns>The files.</returns>
		/// <param name="username">Username.</param>
		/// <param name="subdir">Subdir.</param>
		public static IEnumerable<FileInfo> GetFiles (string username, string subdir)
		{
			return FileManager.GetFiles (Path.Combine(UserFileRoot(username),subdir));
		}

		/// <summary>
		/// Put the specified destDir and files.
		/// </summary>
		/// <param name="destDir">Destination dir.</param>
		/// <param name="files">Files.</param>
		public static void Put(string destDir, NameObjectCollectionBase files)
		{
			FileManager.ValidateSubDir (destDir);
			FileManager.Put(
				Path.Combine(CurrentUserFileRoot(),destDir),
				files);
		}

		/// <summary>
		/// Detail the specified filePath and username.
		/// </summary>
		/// <param name="filePath">File path.</param>
		/// <param name="username">Username.</param>
		public static FileInfo Detail(string filePath, string username=null)
		{
			return FileManager.FileInfo (UserFileRoot (username));
		}
	}
}

