//
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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Owin;
using Microsoft.Owin.Extensions;

namespace Yavsc.ApiControllers
{
	public class ChallengeResult : IHttpActionResult
	{
		public ChallengeResult(string loginProvider, ApiController controller)
		{
			LoginProvider = loginProvider;
			Request = controller.Request;
		}

		public string LoginProvider { get; set; }
		public HttpRequestMessage Request { get; set; }

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			Request.GetOwinContext().Authentication.Challenge(LoginProvider);

			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
			response.RequestMessage = Request;
			return Task.FromResult(response);
		}
	}

}
