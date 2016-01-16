//
//  YavscOAuthServerProvider.cs
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
using Microsoft.Owin.Security.OAuth;
using System.Threading.Tasks;
using System.Security.Claims;

namespace SignalRSelfHost
{
	public class YavscOAuthServerProvider : OAuthAuthorizationServerProvider
	{
		public override async System.Threading.Tasks.Task ValidateClientAuthentication (OAuthValidateClientAuthenticationContext context)
		{
			await Task.FromResult(context.Validated());
		}

		public override async Task GrantResourceOwnerCredentials(
			OAuthGrantResourceOwnerCredentialsContext context)
		{
			// context.UserName
			// DEMO ONLY: Pretend we are doing some sort of REAL checking here:
			if (context.Password != "password")
			{
				context.SetError(
					"invalid_grant", "The user name or password is incorrect.");
				context.Rejected();
				return;
			}

			// Create or retrieve a ClaimsIdentity to represent the 
			// Authenticated user:
			ClaimsIdentity identity = 
				new ClaimsIdentity(context.Options.AuthenticationType);
			identity.AddClaim(new Claim("user_name", context.UserName));

			// Identity info will ultimately be encoded into an Access Token
			// as a result of this call:
			context.Validated(identity);
		}
	}
}

