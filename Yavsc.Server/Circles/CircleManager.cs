//
//  CircleManager.cs
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
using System.Security.Permissions;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Web;
using System.Linq;

namespace Yavsc.Model.Circles
{
	/// <summary>
	/// Circle manager.
	/// </summary>
	public class CircleManager
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Circles.CircleManager"/> class.
		/// </summary>
		public CircleManager ()
		{
		}
		private static CircleProvider defaultProvider=null;
		/// <summary>
		/// Gets the default provider.
		/// </summary>
		/// <value>The default provider.</value>
		public static CircleProvider DefaultProvider {
			get {
				if (defaultProvider == null)
					GetProviderSettings ();
				return defaultProvider;
			}
		}

		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		private static void GetProviderSettings()
		{
			System.Configuration.Configuration config =
				ConfigurationManager.OpenExeConfiguration(
					ConfigurationUserLevel.None);

			DataProviderConfigurationSection pSection =
				config.GetSection("system.web/circleProviders")
				as DataProviderConfigurationSection;
			if (pSection == null)
				throw new ConfigurationErrorsException ("no circleProviders section defined");
			

			ProviderSettingsCollection providerSettings =
				pSection.Providers;
			if (pSection.DefaultProvider != null) {
				var pSetDef = providerSettings [pSection.DefaultProvider];
				ConstructorInfo ci = Type.GetType (pSetDef.Type).GetConstructor (Type.EmptyTypes);
				defaultProvider = ci.Invoke (Type.EmptyTypes) as CircleProvider;
				defaultProvider.Initialize (pSection.DefaultProvider,pSetDef.Parameters);
			}

		}
		/// <summary>
		/// Adds the user to circle.
		/// </summary>
		/// <param name="circleOwner">Circle owner.</param>
		/// <param name="circleName">Circle name.</param>
		/// <param name="userName">User name.</param>
		public static void AddUserToCircle(string circleOwner, string circleName, string userName)
		{
			var id = DefaultProvider.GetId (circleName, circleOwner);
			if (id <= 0)
				throw new InvalidOperationException ("not a circle");
			DefaultProvider.AddMember (id, userName);
		}
		public static void RemoveUserFromCircle(string circleOwner, string circleName, string userName)
		{
			if (string.IsNullOrEmpty (circleOwner))
				throw new InvalidOperationException ("circleOwner is empty");
			if (string.IsNullOrEmpty (circleName))
				throw new InvalidOperationException ("circleName is empty");
			if (string.IsNullOrEmpty (userName))
				throw new InvalidOperationException ("userName is empty");
			var id = DefaultProvider.GetId (circleName, circleOwner);
			if (id <= 0)
				throw new InvalidOperationException ("not a circle");
			DefaultProvider.RemoveMembership (id, userName);
		}
		public string [] Circles (string userName)
		{
			return DefaultProvider.List (userName).Select (x =>
				x.Title).ToArray ();
		}
	}
}

