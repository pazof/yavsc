//
//  SkillManager.cs
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
using System.Configuration;
using System.Reflection;
using System.Configuration.Provider;
using Yavsc.Model.FrontOffice;

namespace Yavsc.Model.Skill
{

	/// <summary>
	/// Skill manager.
	/// </summary>
	public static class SkillManager 
	{
		private static SkillProvider provider = null;
		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <value>The provider.</value>
		private static SkillProvider Provider { 
			get {
				if (provider == null)
					provider = ManagerHelper.GetDefaultProvider<SkillProvider>
						("system.web/skillProviders"); 
				return provider;
			} 
		}

		/// <summary>
		/// Create or modifies the specified skill.
		/// </summary>
		/// <param name="skill">the skill.</param>
		public static long DeclareSkill(SkillEntity skill) {
			Provider.Declare(skill);
			return skill.Id;
		}
		/// <summary>
		/// Declares the skill.
		/// </summary>
		/// <returns>The skill.</returns>
		/// <param name="dec">Dec.</param>
		public static long DeclareUserSkill(UserSkillDeclaration dec) {
			return Provider.Declare(dec);
		}

		/// <summary>
		/// Rates the user skill.
		/// </summary>
		/// <returns>The user skill.</returns>
		/// <param name="username">Username.</param>
		/// <param name="userSkill">User skill.</param>
		public static long RateUserSkill(string username, UserSkillRating userSkill) {
			return Provider.Rate(userSkill);
		}
		/// <summary>
		/// Rates the skill.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="skill">Skill.</param>
		public static void RateSkill(string username, SkillRating skill) {
			Provider.Rate(skill);
		}

		/// <summary>
		/// Finds the skills.
		/// </summary>
		/// <returns>The skill identifier.</returns>
		/// <param name="pattern">Pattern.</param>
		public static SkillEntity [] FindSkill(string pattern){
			return Provider.FindSkill(pattern);
		}

		/// <summary>
		/// Finds the performer.
		/// </summary>
		/// <returns>The performer.</returns>
		/// <param name="skillIds">Skill identifiers.</param>
		public static string [] FindPerformer(long []skillIds){
			return Provider.FindPerformer(skillIds);
		}
		/// <summary>
		/// Deletes the skill.
		/// </summary>
		/// <param name="skillId">Skill identifier.</param>
		public static void DeleteSkill (long skillId)
		{ 
			Provider.DeleteSkill (skillId);
		}
		/// <summary>
		/// Deletes the user skill.
		/// </summary>
		/// <param name="skillId">Skill identifier.</param>
		public static void DeleteUserSkill (long skillId)
		{ 
			Provider.DeleteUserSkill (skillId);
		}
		/// <summary>
		/// Gets the user skills.
		/// </summary>
		/// <returns>The user skills.</returns>
		/// <param name="userName">User name.</param>
		public static PerformerProfile GetUserSkills(string userName)
		{
			return Provider.GetUserSkills (userName);
		}
	}
}

