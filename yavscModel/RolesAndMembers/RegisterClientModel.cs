//
//  RegisterClientModel.cs
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
	/// Register client model.
	/// </summary>
	public class RegisterClientModel : RegisterModel
	{
		/// <summary>
		/// Gets or sets the full name.
		/// </summary>
		/// <value>The full name.</value>
		[DisplayName("Nom complet")]
		[Required(ErrorMessage="S'il vous plait, saisissez le nom complet"),
			RegularExpression("([a-z]|[A-Z]|[\\s-_.~]|[0-9])+")]
		public string Name { get; set; }
		/// <summary>
		/// Gets or sets the address.
		/// </summary>
		/// <value>The address.</value>
		[DisplayName("Addresse")]
		public string Address { get; set; }
		/// <summary>
		/// Gets or sets the state of the city and.
		/// </summary>
		/// <value>The state of the city and.</value>
		[DisplayName("Ville")]
		public string CityAndState { get; set; }
		/// <summary>
		/// Gets or sets the zip code.
		/// </summary>
		/// <value>The zip code.</value>
		[DisplayName("Code postal")]
		public string ZipCode { get; set; }
		/// <summary>
		/// Gets or sets the phone.
		/// </summary>
		/// <value>The phone.</value>
		[DisplayName("Téléphone fixe")]
		public string Phone { get; set; }
		/// <summary>
		/// Gets or sets the mobile.
		/// </summary>
		/// <value>The mobile.</value>
		[DisplayName("Téléphone mobile")]
		public string Mobile { get; set; }

		public string Question { get; set; }

		public string Answer { get; set; }
	}
}
