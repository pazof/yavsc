//
//  AuthorizeAttribute.cs
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
using System.Security.Claims;

public class AuthorizeClaimsAttribute : AuthorizeAttribute
{
	public override bool AuthorizeHubMethodInvocation (Microsoft.AspNet.SignalR.Hubs.IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
	{
		return base.AuthorizeHubMethodInvocation (hubIncomingInvokerContext, appliesToMethod);
	}

	public override bool AuthorizeHubConnection (Microsoft.AspNet.SignalR.Hubs.HubDescriptor hubDescriptor, IRequest request)
	{
		// TODO hack request.Cookies?

		 
		if (request.User == null)
		{
			throw new InvalidOperationException ();
		}
		var principal = request.User as ClaimsPrincipal;

		if (principal != null)
		{
			Claim authenticated = principal.FindFirst(ClaimTypes.Authentication);
			if (authenticated != null && authenticated.Value == "true")
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			return false;
		}

	}
}
