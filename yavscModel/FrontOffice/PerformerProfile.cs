//
//  PerformerProfile.cs
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

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Performer profile.
	/// </summary>
	public class PerformerProfile: UserNameBase, IRating, IIdentified<long>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Skill.PerformerProfile"/> class.
		/// </summary>
		public PerformerProfile()
		{
		}

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public long Id { get; set; }

		public int Rate { get; set; }

		/// <summary>
		/// Gets or sets the skills.
		/// </summary>
		/// <value>The skills.</value>
		public virtual IEnumerable<UserSkill> Skills { get; set; }
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Skill.PerformerProfile"/> class.
		/// </summary>
		/// <param name="username">Username.</param>
		public PerformerProfile(string username)
		{
			UserName = username;
		}

		/// <summary>
		/// Gets the user.
		/// </summary>
		/// <returns>The user.</returns>
		public MembershipUser GetUser()
		{
			return Membership.GetUser (UserName);
		}
		private Profile yavscCLientProfile = null;

		/// <summary>
		/// Gets the yavsc Client profile.
		/// </summary>
		/// <value>The yavsc Client profile.</value>
		public Profile YavscClientProfile
		{
			get { 
				if (yavscCLientProfile == null)
				if (UserName == null)
					throw new Exception ("UserName not set");
				else 
					yavscCLientProfile = new Profile (
						ProfileBase.Create (UserName));
				return yavscCLientProfile;
			}
		}

		public string EMail { 
			get ;
			set; 
		}

		public string GoogleRegId { 
			get { return YavscClientProfile.GoogleRegId; }
		}		

		public string GoogleCalId { 
			get { return YavscClientProfile.GoogleCalendar; }
		}		
		/// <summary>
		/// Determines whether this instance references the specified skillId.
		/// </summary>
		/// <returns><c>true</c> if this instance has skill the specified skillId; otherwise, <c>false</c>.</returns>
		/// <param name="skillId">Skill identifier.</param>
		public bool HasSkill(long skillId)
		{
			return Skills.Any (x => x.SkillId == skillId);
		}

		/// <summary>
		/// Gets the avatar.
		/// </summary>
		/// <value>The avatar.</value>
		public string Avatar { 
			get { 
				return YavscClientProfile.avatar;
			}
		}


		/// <summary>
		/// Determines whether this instance has calendar.
		/// </summary>
		/// <returns><c>true</c> if this instance has calendar; otherwise, <c>false</c>.</returns>
		public bool HasCalendar () 
		{ 
				return (YavscClientProfile.GoogleCalendar != null);
		}

		/// <summary>
		/// Creates the availability description object.
		/// </summary>
		/// <returns>The availability.</returns>
		/// <param name="date">Date.</param>
		/// <param name="available">If set to <c>true</c> available.</param>
		public  PerformerAvailability CreateAvailability(DateTime date, bool available) {
			PerformerAvailability p = new PerformerAvailability(this,UserName,date,available);
			p.DateAvailable = available;
			p.PerformanceDate = date;
			return p;
		}
	}
}

