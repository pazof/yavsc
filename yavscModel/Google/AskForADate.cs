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

namespace Yavsc.Model.Google
{
	/// <summary>
	/// Ask for A date.
	/// </summary>
	public class AskForADate
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Google.AskForADate"/> class.
		/// </summary>
		public AskForADate ()
		{
			MinDate = MaxDate = DateTime.Now.AddMinutes (5);
		}

		/// <summary>
		/// Gets or sets the minimum date.
		/// </summary>
		/// <value>The minimum date.</value>
		[Display(Name="MinDate",ResourceType=typeof(LocalizedText))]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
		[Required(ErrorMessage = "S'il vous plait, saisissez une date minimale au format jj/mm/aaaa")]
		public DateTime MinDate { get; set; }

		/// <summary>
		/// Gets or sets the max date.
		/// </summary>
		/// <value>The max date.</value>
		[Display(Name="MaxDate",ResourceType=typeof(LocalizedText))]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
		public DateTime MaxDate { get; set; }

		/// <summary>
		/// Gets or sets the minimum time.
		/// </summary>
		/// <value>The minimum time.</value>
		[Required(ErrorMessage = "S'il vous plait, saisissez une heure minimale au format hh:mm sur 24 heures")]
		[RegularExpression("\\d\\d:\\d\\d")]
		public string MinTime { get; set; }

		/// <summary>
		/// Gets or sets the max time.
		/// </summary>
		/// <value>The max time.</value>
		[RegularExpression("\\d\\d:\\d\\d")]
		public string MaxTime { get; set; }

		/// <summary>
		/// Gets or sets the duration.
		/// </summary>
		/// <value>The duration.</value>
		[RegularExpression("\\d\\d:\\d\\d")]
		public string Duration { get; set; }

		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		[Required(ErrorMessage="S'il vous plait, saisisser le pseudo de votre interlocuteur")]
		[Display(Name="Consultant",ResourceType=typeof(LocalizedText))]
		public string UserName { get; set; }
	}
}

