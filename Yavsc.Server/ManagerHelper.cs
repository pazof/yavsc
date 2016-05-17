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
using System.Collections.Specialized;
using System.Linq;
using System.Collections.Generic;

namespace Yavsc.Model {
	/// <summary>
	/// Manager.
	/// </summary>
	public static class ManagerHelper { 

		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <value>The provider.</value>
		public static TProvider CreateDefaultProvider<TProvider> (string configSetion) where TProvider: ProviderBase 
		{
			var config = GetConfiguration (configSetion);
			ProviderSettings celt = 
				config.Providers [config.DefaultProvider];
			return CreateProvider<TProvider> (celt.Type, celt.Name, celt.Parameters);
		}

		/// <summary>
		/// Creates the providers.
		/// </summary>
		/// <returns>The providers.</returns>
		/// <param name="configSetion">Config setion.</param>
		/// <typeparam name="TProvider">The 1st type parameter.</typeparam>
		public static TProvider [] CreateProviders<TProvider> (string configSetion) where TProvider: ProviderBase 
		{
			var config = GetConfiguration (configSetion);
			List<TProvider> providers = new List<TProvider> ();
			foreach (ProviderSettings provConfig in config.Providers) {
				var prov = CreateProvider<TProvider> 
				(provConfig.Type, provConfig.Name, provConfig.Parameters);
				providers.Add (prov);
			}
			return providers.ToArray ();
		}

		private static DataProviderConfigurationSection GetConfiguration(string configSetion) {
			DataProviderConfigurationSection config = ConfigurationManager.GetSection (configSetion) as DataProviderConfigurationSection;
			if (config == null)
				throw new ConfigurationErrorsException (
					string.Format( 
						"The providers configuration bloc `{0}` was not found",
						configSetion));
			return config;
		}

		private static TProvider CreateProvider<TProvider>(string type, string name, 
			NameValueCollection config) where TProvider: ProviderBase {
			Type provtype = Type.GetType (type);
			if (provtype == null)
				throw new ProviderException (
					string.Format (
						"Provider type '{0}' was not found",type));
			ConstructorInfo ci = provtype.GetConstructor (Type.EmptyTypes);
			TProvider bp = ci.Invoke (Type.EmptyTypes) as TProvider;
			bp.Initialize (name, config);
			return bp;
		}

		/// <summary>
		/// Gets the default provider.
		/// </summary>
		/// <returns>The default provider.</returns>
		/// <param name="configSetion">Config setion.</param>
		public static ProviderBase CreateDefaultProvider (string configSetion) 
		{
			return CreateDefaultProvider<ProviderBase>(configSetion);
		}
		/// <summary>
		/// Creates the providers.
		/// </summary>
		/// <returns>The providers.</returns>
		/// <param name="configSetion">Config setion.</param>
		public static ProviderBase[] CreateProviders (string configSetion) 
		{
			return CreateProviders<ProviderBase>(configSetion);
		}

	}
}
