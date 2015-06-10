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

			ProtectedConfigurationSection pSection =
				config.GetSection("circleProviders")
				as ProtectedConfigurationSection;
			

			ProviderSettingsCollection providerSettings =
				pSection.Providers;
			if (pSection.DefaultProvider != null) {
				ConstructorInfo ci = Type.GetType (providerSettings [pSection.DefaultProvider].Type).GetConstructor (Type.EmptyTypes);
				defaultProvider = ci.Invoke (Type.EmptyTypes) as CircleProvider;
			}
			/*

			foreach (ProviderSettings pSettings in
				providerSettings)


			{


				Console.WriteLine(
					"Provider settings name: {0}",
					pSettings.Name);


				Console.WriteLine(
					"Provider settings type: {0}",
					pSettings.Type);

				NameValueCollection parameters =
					pSettings.Parameters;

				IEnumerator pEnum =
					parameters.GetEnumerator();

				int i = 0;
				while (pEnum.MoveNext())
				{
					string pLength =
						parameters[i].Length.ToString();
					Console.WriteLine(
						"Provider ssettings: {0} has {1} parameters",
						pSettings.Name, pLength);

				}


			}
			*/

		}

	}
}

