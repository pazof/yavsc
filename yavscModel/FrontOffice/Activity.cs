//
//  Activity.cs
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

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Activity.
	/// </summary>
	public class Activity:  IComment<string>, ITitle
	{
		#region ITitle implementation
		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public string Title {
			get;
			set;
		}
		#endregion

		#region IComment implementation
		/// <summary>
		/// Gets or sets the comment.
		/// </summary>
		/// <value>The comment.</value>
		public string Comment {
			get;
			set;
		}
		#endregion
		#region IIdentified implementation
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string Id {
			get;
			set;
		}
		#endregion
		/// <summary>
		/// Gets or sets the type of the command.
		/// </summary>
		/// <value>The type of the command.</value>
		public Type CommandType {
			get;
			set;
		}
		/// <summary>
		/// The activity object has a static value during the
		/// most of the application life,
		/// They are not supposed to vary, they should are legal values.
		/// As a result, they are identified by their identifier, 
		/// and are said equal since their id are equal.
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Yavsc.Model.FrontOffice.Activity"/>.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Yavsc.Model.FrontOffice.Activity"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="Yavsc.Model.FrontOffice.Activity"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj)
		{
			if (base.Equals (obj))
				return true;
			if (obj == null)
				return false;
			if (GetType().IsAssignableFrom(obj.GetType()))
				if (((Activity)obj).Id == this.Id)
					return true;
			return false;
		}
	}
}

