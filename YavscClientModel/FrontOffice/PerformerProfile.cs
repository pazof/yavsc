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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using YavscClientModel.Accounts;
using YavscClientModel.Skills;

namespace YavscClientModel.FrontOffice
{
	/// <summary>
	/// Performer profile.
	/// </summary>
	public class PerformerProfile: UserNameBase, IRating, IIdentified<long>
	{

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
			if (string.IsNullOrWhiteSpace (username))
				throw new InvalidOperationException (
					"The specified username cannot be blank.");
			UserName = username;
		}

		/// <summary>
		/// Gets or sets the E mail.
		/// </summary>
		/// <value>The E mail.</value>
		public string EMail { 
			get ;
			set; 
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
		/// Gets or sets the MEA code.
		/// </summary>
		/// <value>The MEA code.</value>
		[Required]
		public string MEACode { get; set; }


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

