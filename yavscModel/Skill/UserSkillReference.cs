//
//  UserSkillReference.cs
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

namespace Yavsc.Model.Skill
{
	/// <summary>
	/// User skill reference.
	/// </summary>
	public class UserSkillReference {
		/// <summary>
		/// Gets or sets the user's skill identifier.
		/// </summary>
		/// <value>The skill identifier.</value>
		public long Id { get;set ; }

		/// <summary>
		/// Gets or sets the skill identifier.
		/// </summary>
		/// <value>The skill identifier.</value>
		public long SkillId { get;set ; }

		/// <summary>
		/// Gets or sets the name of the performer.
		/// </summary>
		/// <value>The name of the user.</value>
		public string Performer  { get; set; }
	}
	
}
