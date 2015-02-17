//
//  InvalidDirNameException.cs
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

namespace Yavsc.Model.FileSystem
{

	/// <summary>
	/// Invalid dir name exception.
	/// </summary>
	public class InvalidDirNameException : Exception {

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FileSystem.InvalidDirNameException"/> class.
		/// </summary>
		/// <param name="dir">Dir.</param>
		public InvalidDirNameException(string dir)
			: base(string.Format( "Invalid directory name : {0}", dir))
		{
		}
	}
	/// <summary>
	/// Invalid file name exception.
	/// </summary>
	public class InvalidFileNameException : Exception {

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FileSystem.InvalidDirNameException"/> class.
		/// </summary>
		/// <param name="fileName">Dir.</param>
		public InvalidFileNameException(string fileName)
			: base(string.Format( "Invalid file name : {0}", fileName))
		{
		}
	}
}
