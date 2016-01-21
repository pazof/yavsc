//
//  MonoDataProtectionProvider.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016 GNU GPL
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
using Owin;
using System.Configuration;
using Microsoft.Owin.Security.DataProtection;
using System.Security.Cryptography;
using System.IO;

namespace Yavsc.Model
{

	public class MonoDataProtectionProvider : IDataProtectionProvider
	{
		private readonly string appName;

		public MonoDataProtectionProvider()
			: this(Guid.NewGuid().ToString())
		{ }

		public MonoDataProtectionProvider(string appName)
		{
			if (appName == null) { throw new ArgumentNullException("appName"); }

			this.appName = appName;
		}

		public IDataProtector Create(params string[] purposes)
		{
			if (purposes == null) { throw new ArgumentNullException("purposes"); }

			return new MonoDataProtector(appName, purposes);
		}
	}
    
}