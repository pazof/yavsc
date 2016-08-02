using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Messaging
{

//
//  BookQueryEvent.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015-2016 GNU GPL
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

public class BookQueryEvent: YaEvent
	{
        public BookQueryEvent() : base("BookQuery")
        {
            
        }
		/// <summary>
		/// The location.
		/// </summary>
		[Display(Name="Location")]
		public Location Location { get; set; }
		/// <summary>
		/// The start date.
		/// </summary>
		[Display(Name="StartDate")]
		public DateTime StartDate { get; set; }

		/// <summary>
		/// Gets or sets the end date.
		/// </summary>
		/// <value>The end date.</value>
		[Display(Name="EndDate")]
		public DateTime EndDate { get; set; }

        public long CommandId { get; set; }
	}

}
