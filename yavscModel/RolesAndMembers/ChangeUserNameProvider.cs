//
//  ChangeUserNameProvider.cs
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

namespace Yavsc.Model.RolesAndMembers
{
	/// <summary>
	/// Change user name provider.
	/// </summary>
	public abstract class ChangeUserNameProvider: ProviderBase 
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.RolesAndMembers.ChangeUserNameProvider"/> class.
		/// </summary>
		public ChangeUserNameProvider ()
		{
			
		}

		public  abstract void ChangeName (string oldName, string newName) ;

		public  abstract bool IsNameAvailable(string name);

	}
}

