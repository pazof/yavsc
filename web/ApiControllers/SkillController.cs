//
//  SkillController.cs
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
using Yavsc.Model.Skill;
using System.Web.Http;
using Yavsc.Model;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Skill controller.
	/// </summary>
	public class SkillController : YavscController
	{
		/// <summary>
		/// Create or update the specified Skill.
		/// </summary>
		/// <param name="s">the Skill objet.</param>
		[ValidateAjaxAttribute,Authorize(Roles="Profiler")]
		public long DeclareSkill (Skill s) {
			if (ModelState.IsValid) { 
				SkillManager.DeclareSkill (s);
			}
			return s.Id;
		}

		/// <summary>
		/// Declares the user's skill,
		/// This call should be in charge of 
		/// the user himself, it's an user
		/// declaration, and as a result,
		/// this only is a declarative caracterisation of
		/// the user's skills.
		/// </summary>
		/// <returns>The skill.</returns>
		/// <param name="dec">Dec.</param>
		[Authorize()]
		public long DeclareUserSkill (UserSkillDeclaration dec) {
			return SkillManager.DeclareUserSkill(dec);
		}

		/// <summary>
		/// Rate the specified user's skill.
		/// A way for an effective client to rate
		/// the service or good he should have got.
		/// This is the king's value
		/// </summary>
		/// <param name="rate">The user skill rating.</param>
		[Authorize()]
		public long RateUserSkill (UserSkillRating rate) {
			return SkillManager.RateUserSkill(User.Identity.Name, rate);
		}

		/// <summary>
		/// Rates the skill.
		/// </summary>
		/// <param name="rate">Skill rating.</param>
		[Authorize()]
		public void RateSkill (SkillRating rate) {
			SkillManager.RateSkill(User.Identity.Name,rate);
		}

		/// <summary>
		/// Finds the skill identifier.
		/// </summary>
		/// <returns>The skill identifier.</returns>
		/// <param name="pattern">Pattern.</param>
		public Skill [] FindSkill (string pattern){
			return SkillManager.FindSkill(pattern);
		}

		/// <summary>
		/// Finds the performer.
		/// </summary> 
		/// <returns>The performer.</returns>
		/// <param name="skillIds">Skill identifiers.</param>
		public string [] FindPerformer (long []skillIds){
			return SkillManager.FindPerformer(skillIds);
		}
		/// <summary>
		/// Deletes the skill.
		/// </summary>
		/// <param name="skillId">Skill identifier.</param>
		[Authorize(Roles="Moderator")]
		public void DeleteSkill (long skillId)
		{ 
			SkillManager.DeleteSkill (skillId);
		}
	}
}

