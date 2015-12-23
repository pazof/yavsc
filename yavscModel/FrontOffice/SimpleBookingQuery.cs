//
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
using Yavsc.Model.Skill;
using Yavsc.Model.WorkFlow;
using System.Linq;
using Yavsc.Model.RolesAndMembers;

namespace Yavsc.Model.FrontOffice
{

	/// <summary>
	/// Simple booking query.
	/// </summary>
	public class SimpleBookingQuery: Command
	{
		#region implemented abstract members of Command

		public override string GetDescription ()
		{
			return 
				string.Format (
					LocalizedText.SomeoneAskingYouForAnEstimate,
				ClientName,
					LocalizedText.aprestation+" "+ PreferedDate.ToString("D"));
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


	}
}

