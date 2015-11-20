//
//  AuthorizeAttribute.cs
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

namespace Yavsc
{
	/// <summary>
	/// Authorize attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class AuthorizeAttribute: System.Web.Mvc.AuthorizeAttribute
	{
		/// <summary>
		/// Handles the unauthorized request.
		/// </summary>
		/// <param name="filterContext">Filter context.</param>
		protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
		{
			if (filterContext.HttpContext.Request.IsAuthenticated)
			{
				if (string.IsNullOrWhiteSpace (Users) && !string.IsNullOrEmpty (Roles))
				{			
					// let the client know which role were allowed here
					// filterContext.ActionDescriptor.ControllerDescriptor. 
					var result = new System.Web.Mvc.ViewResult();
					
					filterContext.Controller.ViewData ["ActionName"] = filterContext.ActionDescriptor.ActionName;
					filterContext.Controller.ViewData ["ControllerName"] = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
					filterContext.Controller.ViewData ["Roles"] = Roles;
					filterContext.Controller.ViewData ["Users"] = Users;
					result.ViewName = "RestrictedArea";
					result.ViewData = filterContext.Controller.ViewData;
					filterContext.Result = result;

				}
				else filterContext.Result = new System.Web.Mvc.HttpStatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
			}
			else
			{
				base.HandleUnauthorizedRequest(filterContext);
			}
		}
	}
}

