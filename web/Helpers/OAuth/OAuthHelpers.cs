//
//  OAuthHelpers.cs
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
using Yavsc.Helpers.OAuth.Api;
using System.Collections.Generic;
using Microsoft.Owin.Security;
using System.Web;

namespace Yavsc.Helpers.OAuth
{
	public static class OAuthHelpers
	{
		public static Dictionary<string,AuthenticationOptions> ExternalAuthOptions = 
			new Dictionary<string, AuthenticationOptions> ();

		public static Dictionary<string,string> ExternalAuthClientId = 
			new Dictionary<string, string> ();

		private static Dictionary<string,OAuthAccessPoint> Authentications = 
			new Dictionary<string,OAuthAccessPoint> ();
		
		public static OAuthAccessPoint GetOAuth(string authType)
		{
			return Authentications[authType];
		}

		public static string SetSessionSate (this HttpSessionStateBase session)
		{
			string state = "security_token";
			Random rand = new Random ();
			for (int l = 0; l < 32; l++) {
				int r = rand.Next (62);
				char c;
				if (r < 10) {
					c = (char)('0' +  r);
				} else if (r < 36) {
					r -= 10;
					c = (char) ('a' + r);
				} else {
					r -= 36;
					c = (char) ('A' + r);
				}
				state += c;
			}
			session ["state"] = state;
			return state;
		}
	}
}

