//
//  Manager.cs
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

namespace Yavsc.Model {
	/// <summary>
	/// Manager.
	/// </summary>
	public static class ManagerHelper { 

		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <value>The provider.</value>
		public static TProvider GetDefaultProvider<TProvider> (string configSetion) where TProvider: ProviderBase 
		{
			DataProviderConfigurationSection config = ConfigurationManager.GetSection (configSetion) as DataProviderConfigurationSection;
			if (config == null)
				throw new ConfigurationErrorsException (
					string.Format( 
						"The providers configuration bloc `{0}` was not found",
						configSetion));
			ProviderSettings celt = 
				config.Providers [config.DefaultProvider];
			if (config == null)
				throw new ConfigurationErrorsException (
					string.Format ( 
						"The default provider `{0}` was not found ", 
						config.DefaultProvider));
			Type provtype = Type.GetType (celt.Type);
			if (provtype == null)
				throw new ProviderException (
					string.Format (
						"Provider type '{0}' was not found",celt.Type));
			ConstructorInfo ci = provtype.GetConstructor (Type.EmptyTypes);
			ProviderBase bp = ci.Invoke (Type.EmptyTypes) as ProviderBase;
			bp.Initialize (celt.Name, celt.Parameters);
			return bp as TProvider;
		}
	}
}
