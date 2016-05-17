//
//  CircleConfigurationSection.cs
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

namespace Yavsc.Model
{
	/// <summary>
	/// Data provider configuration section.
	/// </summary>
	public class DataProviderConfigurationSection: ConfigurationSection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.DataProviderConfigurationSection"/> class.
		/// </summary>
		public DataProviderConfigurationSection ()
		{
		}

		/// <summary>
		/// Gets or sets the default provider.
		/// </summary>
		/// <value>The default provider.</value>
		[ConfigurationProperty ("defaultProvider")]
		public string DefaultProvider { 
			get {
				return (string)base ["defaultProvider"];
			}
			set {
				base ["defaultProvider"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the providers.
		/// </summary>
		/// <value>The providers.</value>
		[ConfigurationProperty ("providers")]
		[ConfigurationCollection(typeof(ProviderSettingsCollection),
			AddItemName = "add",
			ClearItemsName = "clear",
			RemoveItemName = "remove")]
		public ProviderSettingsCollection Providers {
			get {
				return (ProviderSettingsCollection)base ["providers"];
			}
			set {
				base ["providers"] = value;
			}
		}

	}
}

