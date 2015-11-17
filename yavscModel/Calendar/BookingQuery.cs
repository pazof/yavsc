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
	public class BookingQuery
	{

		/// <summary>
		/// Gets or sets the prefered date.
		/// </summary>
		/// <value>The prefered date.</value>
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
		[Display(ResourceType=typeof(LocalizedText),Name="StartDate")]
		public DateTime StartDate { get; set; }

		/// <summary>
		/// Gets or sets the minimum time.
		/// </summary>
		/// <value>The minimum time.</value>
		[RegularExpression("\\d\\d:\\d\\d")]
		[Display(ResourceType=typeof(LocalizedText),Name="StartHour")]
		[Required(ErrorMessage= "S'il vous plait, saisissez une heure de début d'intervention")]
		public string StartHour { get; set; }

		/// <summary>
		/// Gets or sets the max date.
		/// </summary>
		/// <value>The max date.</value>
		[Display(Name="EndDate",ResourceType=typeof(LocalizedText))]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
		public DateTime EndDate { get; set; }


		/// <summary>
		/// Gets or sets the minimal duration.
		/// </summary>
		/// <value>The duration.</value>
		[RegularExpression("\\d\\d:\\d\\d")]
		[Required(ErrorMessage= "S'il vous plait, saisissez une heure de fin d'intervention")]
		[Display(Name="EndHour",ResourceType=typeof(LocalizedText))]
		public string EndHour { get; set; }

		/// <summary>
		/// Gets or sets the role.
		/// </summary>
		/// <value>The role.</value>
		[Required(ErrorMessage= "S'il vous plait, saisissez le ou les types d'intervention souhaité")]
		[Display(Name="Role",ResourceType=typeof(LocalizedText))]
		public string [] Roles { get; set; }
	}
}

