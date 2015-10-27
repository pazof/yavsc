//
//  AskForADate.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2014 Paul Schneider
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Calendar
{
	/// <summary>
	/// Ask for A date.
	/// </summary>
	public class BookQuery
	{

		/// <summary>
		/// Gets or sets the prefered date.
		/// </summary>
		/// <value>The prefered date.</value>
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
		public DateTime PreferedDate { get; set; }

		/// <summary>
		/// Gets or sets the minimum time.
		/// </summary>
		/// <value>The minimum time.</value>
		[RegularExpression("\\d\\d:\\d\\d")]
		public string PreferedHour { get; set; }

		/// <summary>
		/// Gets or sets the max date.
		/// </summary>
		/// <value>The max date.</value>
		[Display(Name="MaxDate",ResourceType=typeof(LocalizedText))]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
		public DateTime MaxDate { get; set; }


		/// <summary>
		/// Gets or sets the minimal duration.
		/// </summary>
		/// <value>The duration.</value>
		[RegularExpression("\\d\\d:\\d\\d")]
		[Required(ErrorMessage= "S'il vous plait, saisissez une durée minimale d'intervention")]
		public string MinDuration { get; set; }

		/// <summary>
		/// Gets or sets the person.
		/// </summary>
		/// <value>The name of the user.</value>
		[Display(Name="Person",ResourceType=typeof(LocalizedText))]
		public string Person { get; set; }

		/// <summary>
		/// Gets or sets the role.
		/// </summary>
		/// <value>The role.</value>
		[Display(Name="Role",ResourceType=typeof(LocalizedText))]
		public string Role { get; set; }
	}
}

