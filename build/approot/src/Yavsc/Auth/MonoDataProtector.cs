//
//  MonoDataProtector.cs
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
using System.Security.Cryptography;
using System.IO;
using Microsoft.AspNet.DataProtection;
using System.Linq;

namespace Yavsc.Auth
{
	public class MonoDataProtector : IDataProtector
	{
		private const string PRIMARY_PURPOSE = "IDataProtector";

		private readonly string appName;
		private readonly DataProtectionScope dataProtectionScope;
		private readonly string[] purposes;

		public MonoDataProtector(string appName, string[] purposes)
		{
			if (appName == null) { throw new ArgumentNullException("appName"); }
			if (purposes == null) { throw new ArgumentNullException("purposes"); }

			this.appName = appName;
			this.purposes = purposes;
			this.dataProtectionScope = DataProtectionScope.CurrentUser;
		}

        public IDataProtector CreateProtector(string purpose)
        {
            if (purposes.Contains(purpose))
                return new MonoDataProtector(appName,new string[] {purpose});
            return new MonoDataProtector(appName,new string[] {});
        }

        public byte[] Protect(byte[] userData)
		{
			return ProtectedData.Protect(userData, this.GetEntropy(), dataProtectionScope);
		}

		public byte[] Unprotect(byte[] protectedData)
		{
			return ProtectedData.Unprotect(protectedData, this.GetEntropy(), dataProtectionScope);
		}

		private byte[] GetEntropy()
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				using (MemoryStream memoryStream = new MemoryStream())
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, sha256, CryptoStreamMode.Write))
				using (StreamWriter writer = new StreamWriter(cryptoStream))
				{
					writer.Write(this.appName);
					writer.Write(PRIMARY_PURPOSE);

					foreach (string purpose in this.purposes)
					{
						writer.Write(purpose);
					}
				}

				return sha256.Hash;
			}
		}
	}

}