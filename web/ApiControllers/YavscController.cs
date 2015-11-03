﻿//
//  YavscApiController.cs
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
using System.Web.Profile;

namespace Yavsc.ApiControllers
{
	public class YavscController : ApiController
	{
		public YavscController ()
		{
		}
		public class Auth { 
			public string Id { get; set; }
		}

		public void AllowCookies (Auth model)
		{
			if (model.Id != null) {
				ProfileBase pr = ProfileBase.Create (model.Id);
				pr.SetPropertyValue ("allowcookies", true);
				pr.Save ();
			}
		}

		protected HttpResponseMessage DefaultResponse()
		{
			return ModelState.IsValid ?
				Request.CreateResponse (System.Net.HttpStatusCode.OK) :
				Request.CreateResponse (System.Net.HttpStatusCode.BadRequest,
					ValidateAjaxAttribute.GetErrorModelObject (ModelState));
		}
	}
}

