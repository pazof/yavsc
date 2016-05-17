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
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Yavsc.Model.Authentication
{
	
	public class YavscAuthenticationOptions: AuthenticationOptions
	{
		public YavscAuthenticationOptions(string authType, string clientId, string clientSecret)
			: base(authType)
		{
			if (authType == null)
				throw new NotSupportedException();
			Description.Caption = authType;
			AuthenticationMode = AuthenticationMode.Passive;
			ClientId = clientId;
			ClientSecret = clientSecret;
			SignInAsAuthenticationType = authType;
			Scope = new List<string>();
		}
		public PathString TokenPath { get; set; }
		public PathString AuthorizePath { get; set; }

		public PathString ReturnUrl { get; set; }
		public PathString LoginPath { get; set; }
		public PathString LogoutPath { get; set; }

		internal string AuthenticationServerUri = "https://accounts.google.com/o/oauth2/auth";
		internal string TokenServerUri = "https://accounts.google.com/o/oauth2/token";

		public string ClientId { get; set; }

		public string ClientSecret { get; set; }

		private string signInAsAuthenticationType = null;
		public string SignInAsAuthenticationType { get
			{ return signInAsAuthenticationType ; }
			set { signInAsAuthenticationType = value;
				ReturnUrl = new PathString("/signin-"+signInAsAuthenticationType.ToLower());
			}  }
		public ICollection<string> Scope { get; set; }
		public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
	}
}

