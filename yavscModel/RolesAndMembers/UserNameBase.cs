//
//  UserNameBase.cs
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

namespace Yavsc.Model.RolesAndMembers
{
	/// <summary>
	/// User name base.
	/// </summary>
	public class UserNameBase : IIdentified<string> { 
		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		[Localizable(true), Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur")
			,Display(ResourceType=typeof(LocalizedText),Name="User_name"),RegularExpression(@"^[a-zA-Z .-_#]{1,100}$")]
		public string UserName { get; set; }
	
		#region IIdentified implementation
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string Id {
			get {
				return UserName;
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
	
}
