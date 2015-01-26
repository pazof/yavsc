//
//  ModuleConfigurationElementCollection.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2014 Paul Schneider
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

namespace Yavsc.Settings
{
	/// <summary>
	/// Module configuration element collection.
	/// </summary>
	public class ModuleConfigurationElementCollection : ConfigurationElementCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Settings.ModuleConfigurationElementCollection"/> class.
		/// </summary>
		public ModuleConfigurationElementCollection ()
		{
		}

		#region implemented abstract members of ConfigurationElementCollection
		/// <summary>
		/// Creates the new element.
		/// </summary>
		/// <returns>The new element.</returns>
		protected override ConfigurationElement CreateNewElement ()
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Gets the element key.
		/// </summary>
		/// <returns>The element key.</returns>
		/// <param name="element">Element.</param>
		protected override object GetElementKey (ConfigurationElement element)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

