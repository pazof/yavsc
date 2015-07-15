﻿//
//  AccountController.cs
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
using System.Web.Http;
using System.Net.Http;
using Yavsc.Model.RolesAndMembers;
using System.Web.Security;
using System.Web.Profile;
using Yavsc.Helpers;
using System.Collections.Specialized;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Account controller.
	/// </summary>
	public class AccountController : ApiController
	{

		/// <summary>
		/// Register the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		[Authorize()]
		[ValidateAjaxAttribute]
		public HttpResponseMessage Register ([FromBody] RegisterClientModel model)
		{
			if (ModelState.IsValid) {
				if (model.IsApprouved)
				if (!Roles.IsUserInRole ("Admin"))
				if (!Roles.IsUserInRole ("FrontOffice")) {
					ModelState.AddModelError ("Register", 
						"Since you're not member of Admin or FrontOffice groups, " +
						"you cannot ask for a pre-approuved registration");
					return DefaultResponse ();
				}
				MembershipCreateStatus mcs;
				var user = Membership.CreateUser (
					model.UserName,
					model.Password,
					model.Email,
					model.Question,
					model.Answer,
					model.IsApprouved,
					out mcs);
				switch (mcs) {
				case MembershipCreateStatus.DuplicateEmail:
					ModelState.AddModelError ("Email", "Cette adresse e-mail correspond " +
						"à un compte utilisateur existant");
					break;
				case MembershipCreateStatus.DuplicateUserName:
					ModelState.AddModelError ("UserName", "Ce nom d'utilisateur est " +
						"déjà enregistré");
					break;
				case MembershipCreateStatus.Success:
					if (!model.IsApprouved)
						YavscHelpers.SendActivationMessage (user);
					ProfileBase prtu = ProfileBase.Create (model.UserName);
					prtu.SetPropertyValue("Name",model.Name);
					prtu.SetPropertyValue("Address",model.Address);
					prtu.SetPropertyValue("CityAndState",model.CityAndState);
					prtu.SetPropertyValue("Mobile",model.Mobile);
					prtu.SetPropertyValue("Phone",model.Phone);
					prtu.SetPropertyValue("ZipCode",model.ZipCode);
					break;
				default:
					break;
				}
			}
			return DefaultResponse ();
		}


		private HttpResponseMessage DefaultResponse()
		{
			return ModelState.IsValid ?
				Request.CreateResponse (System.Net.HttpStatusCode.OK) :
				Request.CreateResponse (System.Net.HttpStatusCode.BadRequest,
					ValidateAjaxAttribute.GetErrorModelObject (ModelState));
		}

		/// <summary>
		/// Resets the password.
		/// </summary>
		/// <param name="model">Model.</param>
		[ValidateAjax]
		public void ResetPassword(LostPasswordModel model)
		{
			StringDictionary errors;
			YavscHelpers.ResetPassword (model, out errors);
			foreach (string key in errors.Keys)
				ModelState.AddModelError (key, errors [key]);
		}
	}
}

