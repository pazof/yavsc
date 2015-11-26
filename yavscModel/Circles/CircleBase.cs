//
//  CircleBase.cs
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

namespace Yavsc.Model.Circles
{
	/// <summary>
	/// Circle base.
	/// </summary>
	public class CircleBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Circles.CircleBase"/> class.
		/// </summary>
		public CircleBase ()
		{
		}

		/// <summary>
		/// Gets or sets the owner.
		/// </summary>
		/// <value>The owner.</value>
		public string Owner { get; set; } 

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
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Yavsc.Model.Circles.CircleBase"/>.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Yavsc.Model.Circles.CircleBase"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="Yavsc.Model.Circles.CircleBase"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj)
		{
			if (base.Equals (obj))
				return true;
			return Id == ((CircleBase)obj).Id;
		}
	}
}

