//
//  YavscAuthenticationMiddlewareOptions.cs
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
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Http;

namespace Yavsc.Authentication
{

	public class YavscAuthenticationOptions: OAuthOptions
	{
		public YavscAuthenticationOptions()
		{}

		public YavscAuthenticationOptions(string authType, string clientId, string clientSecret)
		{
			if (authType == null)
				throw new NotSupportedException();
			Description.AuthenticationScheme = authType;
			ClientId = clientId;
			ClientSecret = clientSecret;
			SignInAsAuthenticationType = authType;
			Scope.Clear();
		}
		public PathString TokenPath { get; set; }
		public PathString AuthorizePath { get; set; }

		public PathString ReturnUrl { get; set; }
		public PathString LoginPath { get; set; }
		public PathString LogoutPath { get; set; }

		internal string AuthenticationServerUri = "https://accounts.google.com/o/oauth2/auth";
		internal string TokenServerUri = "https://accounts.google.com/o/oauth2/token";

		private string signInAsAuthenticationType = null;
		public string SignInAsAuthenticationType { get
			{ return signInAsAuthenticationType ; }
			set { signInAsAuthenticationType = value;
				ReturnUrl = new PathString("/signin-"+signInAsAuthenticationType.ToLower());
			}  }

	}
}

