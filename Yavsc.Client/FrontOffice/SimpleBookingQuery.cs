﻿//
//  SimpleBookingQuery.cs
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
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Yavsc.Client.FrontOffice
{

	/// <summary>
	/// Simple booking query.
	/// </summary>
	public class SimpleBookingQuery: Command, ILocation
	{
		#region implemented abstract members of Command

		public override string GetDescription ()
		{
			string result = string.Format(
				LocalizedText.SomeoneAskingYouForAnEstimate,
				ClientName,
				LocalizedText.aprestation);


			result+=string.Format(" {0};", PreferedDate.ToString("D"));
			result+=string.Format(LocalizedText.PresationLocation, Address);
			return result;
		}

		#endregion

		/// <summary>
		/// Gets or sets the MEA code.
		/// </summary>
		/// <value>The MEA code.</value>
		[Required(ErrorMessageResourceName="MEACode",ErrorMessageResourceType=typeof(LocalizedText))]
		public string MEACode { 
			get;
			set;
		}
		/// <summary>
		/// Gets or sets the prefered date.
		/// </summary>
		/// <value>The prefered date.</value>
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
		[Display(ResourceType=typeof(LocalizedText),Name="PreferedDate")]
		public DateTime PreferedDate { get; set; }

		/// <summary>
		/// Gets or sets the needs.
		/// </summary>
		/// <value>The needs.</value>
		[Display(ResourceType=typeof(LocalizedText),Name="Need")]
		public string Need { get; set; }

		/// <summary>
		/// Gets or sets the address.
		/// </summary>
		/// <value>The address.</value>
		public string Address { get; set; }

		/// <summary>
		/// Gets or sets the latitude.
		/// </summary>
		/// <value>The latitude.</value>
		public double Latitude { get; set; }

		/// <summary>
		/// Gets or sets the longitude.
		/// </summary>
		/// <value>The longitude.</value>
		public double Longitude { get; set; }

	}
}
