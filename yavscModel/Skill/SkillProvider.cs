//
//  SkillProvider.cs
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
using System.Configuration.Provider;
using Yavsc.Model.FrontOffice;

namespace Yavsc.Model.Skill
{
	/// <summary>
	/// Skill provider.
	/// </summary>
	public abstract class SkillProvider: ProviderBase
	{
		/// <summary>
		/// Declare the specified skill.
		/// </summary>
		/// <param name="skill">Skill.</param>
		public abstract long Declare(SkillEntity skill) ;

		/// <summary>
		/// Declare the specified user skill.
		/// </summary>
		/// <param name="userskill">Userskill.</param>
		public abstract long Declare(UserSkillDeclaration userskill) ;

		/// <summary>
		/// Rate the specified user skill.
		/// </summary>
		/// <param name="userskill">Userskill.</param>
		public abstract long Rate(UserSkillRating userskill) ;

		/// <summary>
		/// Rate the specified skill.
		/// </summary>
		/// <param name="skill">Skill.</param>
		public abstract void Rate(SkillRating skill) ;

		/// <summary>
		/// Finds the skill identifier.
		/// </summary>
		/// <returns>The skill identifier.</returns>
		/// <param name="pattern">Pattern.</param>
		public abstract SkillEntity [] FindSkill(string pattern, string MEACode);

		/// <summary>
		/// Gets the user skills.
		/// </summary>
		/// <returns>The user skills.</returns>
		/// <param name="username">Username.</param>
		public abstract PerformerProfile GetUserSkills(string username) ;

		/// <summary>
		/// Finds the performer.
		/// </summary>
		/// <returns>The performer.</returns>
		/// <param name="skillIds">Skill identifiers.</param>
		public abstract string [] FindPerformer(long []skillIds);

		/// <summary>
		/// Deletes the skill.
		/// </summary>
		/// <param name="skillId">Skill identifier.</param>
		public abstract void DeleteSkill(long skillId);

		/// <summary>
		/// Deletes the user skill.
		/// </summary>
		/// <param name="userSkillId">User skill identifier.</param>
		public abstract void DeleteUserSkill(long userSkillId);
	}
}

