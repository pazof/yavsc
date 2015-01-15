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
	public class AskForADate
	{
		public AskForADate ()
		{
			MinDate = MaxDate = DateTime.Now.AddMinutes (5);
		}

		[Display(Name="MinDate",ResourceType=typeof(LocalizedText))]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
		[Required(ErrorMessage = "S'il vous plait, saisissez une date minimale au format jj/mm/aaaa")]
		public DateTime MinDate { get; set; }

		[Display(Name="MaxDate",ResourceType=typeof(LocalizedText))]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
		public DateTime MaxDate { get; set; }

		[Required(ErrorMessage = "S'il vous plait, saisissez une heure minimale au format hh:mm sur 24 heures")]
		[RegularExpression("\\d\\d:\\d\\d")]
		public string MinTime { get; set; }

		[RegularExpression("\\d\\d:\\d\\d")]
		public string MaxTime { get; set; }

		[RegularExpression("\\d\\d:\\d\\d")]
		public string Duration { get; set; }

		[Required(ErrorMessage="S'il vous plait, saisisser le pseudo de votre interlocuteur")]
		[Display(Name="Consultant",ResourceType=typeof(LocalizedText))]
		public string UserName { get; set; }
	}
}

