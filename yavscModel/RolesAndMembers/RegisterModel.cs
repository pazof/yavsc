//
//  RegisterModel.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 Paul Schneider
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

namespace Yavsc.Model.RolesAndMembers
{

	/// <summary>
	/// Register view model.
	/// </summary>
	public class RegisterModel
	{
		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		[Localizable(true), Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur")
			,Display(ResourceType=typeof(LocalizedText),Name="User_name")]
		public string UserName { get; set; }
 
		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>The password.</value>
		[DisplayName("Mot de passe")]
		[Required(ErrorMessage = "S'il vous plait, entez un mot de passe")]
		public string Password { get; set; }
 
		/// <summary>
		/// Gets or sets the email.
		/// </summary>
		/// <value>The email.</value>
		[DisplayName("Adresse e-mail")]
		[Required(ErrorMessage = "S'il vous plait, entrez un e-mail valide")]
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is approuved.
		/// </summary>
		/// <value><c>true</c> if this instance is approuved; otherwise, <c>false</c>.</value>
		public bool IsApprouved { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.RolesAndMembers.RegisterModel"/> class.
		/// </summary>
		public RegisterModel()
		{
			IsApprouved = false;
		}
	}
	
}
