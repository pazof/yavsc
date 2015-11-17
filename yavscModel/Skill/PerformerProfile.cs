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

namespace Yavsc.Model.Skill
{
	/// <summary>
	/// Performer profile.
	/// </summary>
	public class PerformerProfile: IRating, IIdentified
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Skill.PerformerProfile"/> class.
		/// </summary>
		public PerformerProfile()
		{
		}

		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		[Localizable(true), Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur")
			,Display(ResourceType=typeof(LocalizedText),Name="User_name"),RegularExpression("([a-z]|[A-Z]|[0-9] )+")]
		public string UserName { get; set; }
		public long Id { get; set; }
		/// <summary>
		/// Gets or sets the skills.
		/// </summary>
		/// <value>The skills.</value>
		public virtual IEnumerable<UserSkill> Skills { get; set; }
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.FrontOffice.PerformerProfile"/> class.
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
		/// Gets the yavsc C lient profile.
		/// </summary>
		/// <value>The yavsc C lient profile.</value>
		public Profile YavscCLientProfile
		{
			get { 
				if (yavscCLientProfile == null)
					yavscCLientProfile = new Profile (
						ProfileBase.Create (UserName));
				return yavscCLientProfile;
			}
		}
		/// <summary>
		/// Gets or sets the rate.
		/// </summary>
		/// <value>The rate.</value>
		public int Rate {
			get ;
			set ;
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
	}
}

