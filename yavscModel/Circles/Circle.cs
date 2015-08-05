//
//  Circle.cs
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
using System.Collections.Generic;

namespace Yavsc.Model.Circles
{
	/// <summary>
	/// Circle.
	/// </summary>
	public class Circle
	{
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public long Id { get; set; }
		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the owner.
		/// </summary>
		/// <value>The owner.</value>
		public string Owner { get; set; } 

		/// <summary>
		/// Gets or sets the users.
		/// </summary>
		/// <value>The users.</value>
		public string [] Members { get; set; }

		/// <summary>
		/// Merge the specified circle array into
		/// an user name list.
		/// </summary>
		/// <param name="those">Those circle about to be merged.</param>
		public static string [] Union (Circle []those)
		{
			List<string> content = new List<string>();
			foreach (Circle c in those) {
				foreach (string user_name in c.Members) {
					if (!content.Contains (user_name))
						content.Add (user_name);
				}
			}
			return content.ToArray ();
		}
		/// <summary>
		/// Gets or sets a value indicating whether this instance is private.
		/// </summary>
		/// <value><c>true</c> if this instance is private; otherwise, <c>false</c>.</value>
		public bool IsPrivate { get; set; }
	}

}

