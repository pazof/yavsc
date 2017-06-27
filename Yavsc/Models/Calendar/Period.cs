//
//  Period.cs
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

namespace Yavsc.Models.Calendar
{
	/// <summary>
	/// Hollydays.
	/// </summary>
	public class Period {
		/// <summary>
		/// Gets or sets the start.
		/// </summary>
		/// <value>The start.</value>
		[Required,Display(Name="Début")]
		public DateTime Start { get; set; }
		/// <summary>
		/// Gets or sets the end.
		/// </summary>
		/// <value>The end.</value>
		[Required,Display(Name="Fin")]
		public DateTime End { get; set; }

		public static Period operator ^ (Period foo, Period bar) {
			var min = ( DateTime.Compare(foo.Start, bar.Start) > 0 ) ? foo.Start : bar.Start;
			var max = ( DateTime.Compare(bar.End, foo.End) > 0 ) ? foo.End : bar.End;
			if (DateTime.Compare(max, min)>0) return new Period { Start = min, End = max };
			return null;
		}
	}

}
