//
//  ProfileEdition.cs
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Profile;
using System.Collections.Generic;
using System.Web.Mvc;
using Yavsc.Model.WorkFlow;

namespace Yavsc.Model.RolesAndMembers
{
	/// <summary>
	/// Profile edition.
	/// </summary>
	public class ProfileEdition : Profile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.RolesAndMembers.ProfileEdition"/> class.
		/// </summary>
		public ProfileEdition()
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.RolesAndMembers.ProfileEdition"/> class.
		/// </summary>
		/// <param name="pr">Pr.</param>
		public ProfileEdition(ProfileBase pr): base(pr)
		{
			NewUserName = UserName;
		}

		/// <summary>
		/// Gets or sets the new name of the user.
		/// </summary>
		/// <value>The new name of the user.</value>
		[Localizable(true), Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur valide")
			,Display(ResourceType=typeof(LocalizedText),Name="User_name"),
			RegularExpression("([a-z]|[A-Z]|[\\s-_.~]|[0-9])+")
		]
		public string NewUserName { get; set; }



	}
}

