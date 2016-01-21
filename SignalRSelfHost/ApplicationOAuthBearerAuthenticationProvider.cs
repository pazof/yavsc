//
//  ApplicationOAuthBearerAuthenticationProvider.cs
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
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using System.Configuration;
using Microsoft.Owin.Security.DataProtection;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Owin.Security.OAuth;
using System.Threading.Tasks;

namespace SignalRSelfHost
{
	public class ApplicationOAuthBearerAuthenticationProvider 
		: OAuthBearerAuthenticationProvider
	{
		public override Task RequestToken(OAuthRequestTokenContext context)
		{
			Console.WriteLine ("ApplicationOAuthBearerAuthenticationProvider");
			if (context == null) throw new ArgumentNullException("context");

			// try to find bearer token in a cookie 
			// (by default OAuthBearerAuthenticationHandler 
			// only checks Authorization header)
			var tokenCookie = context.OwinContext.Request.Cookies["BearerToken"];
			if (!string.IsNullOrEmpty(tokenCookie))
				context.Token = tokenCookie;
			return Task.FromResult<object>(null);
		}

	}
    
}