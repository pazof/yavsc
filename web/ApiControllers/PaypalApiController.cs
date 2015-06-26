//
//  PaypalApiController.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 Paul Schneider
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


using PayPal;
using System.Collections.Generic;
using PayPal.OpenIdConnect;
using PayPal.Manager;
using PayPal.PayPalAPIInterfaceService;
using PayPal.PayPalAPIInterfaceService.Model;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Paypal API controller.
	/// </summary>
	public class PaypalApiController: ApiController
	{
		PayPalAPIInterfaceServiceService service = null; 
		/// <summary>
		/// Initialize the specified controllerContext.
		/// </summary>
		/// <param name="controllerContext">Controller context.</param>
		protected override void Initialize (System.Web.Http.Controllers.HttpControllerContext controllerContext)
		{
			base.Initialize (controllerContext);
			// Get the config properties from PayPal.Api.ConfigManager
			// Create the Classic SDK service instance to use.
			service = new PayPalAPIInterfaceServiceService(ConfigManager.Instance.GetProperties());
		}
	/// <summary>
	/// Search the specified str.
	/// </summary>
	/// <param name="str">str.</param>
		public BMCreateButtonResponseType Create(string str)
		{
			BMCreateButtonRequestType btcrerqu = new BMCreateButtonRequestType ();
			BMCreateButtonReq btcrerq = new BMCreateButtonReq ();
			btcrerq.BMCreateButtonRequest = btcrerqu;
			BMCreateButtonResponseType btcrere = service.BMCreateButton (btcrerq);
			return btcrere;
		}

		public BMButtonSearchResponseType Search(string str)
		{
			BMButtonSearchReq req = new BMButtonSearchReq ();
			req.BMButtonSearchRequest = new BMButtonSearchRequestType ();
			
			return service.BMButtonSearch (req);
		}
	}

}

