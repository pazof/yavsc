//
//  PerformerAvailability.cs
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
using Yavsc.Model.RolesAndMembers;
using Yavsc.Model.Skill;
using System.Web.Security;
using System.Web.Profile;
using System.Collections.Generic;
using Yavsc.Model;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Yavsc.Model.Google.Api;
using Yavsc.Model.WorkFlow;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Performer availability.
	/// </summary>
	public class PerformerAvailability : UserNameBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.PerformerAvailability"/> class.
		/// </summary>
		/// <param name="profile">Profile.</param>
		/// <param name="userName">User name.</param>
		/// <param name="date">Date.</param>
		/// <param name="available">If set to <c>true</c> available.</param>
		public PerformerAvailability(PerformerProfile profile,
			string userName, DateTime date, bool available) {
			DateAvailable = available;
			PerformanceDate = date;
			UserName = userName;
			preformerProfile = profile;
		}

		PerformerProfile preformerProfile = null;
		/// <summary>
		/// Gets the profile.
		/// </summary>
		/// <value>The profile.</value>
		public PerformerProfile Profile
		{ 
			get { 
				return preformerProfile;
			}
		}

		/// <summary>
		/// Gets or sets the performance date.
		/// </summary>
		/// <value>The performance date.</value>
		public DateTime PerformanceDate { get; set; }
	
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.FrontOffice.PerformerAvailability"/> date available.
		/// </summary>
		/// <value><c>true</c> if date available; otherwise, <c>false</c>.</value>
		public bool DateAvailable { get; set; }
	
	}

}
