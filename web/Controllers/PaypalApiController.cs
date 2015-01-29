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

#if HASPAYPALAPI

using PayPal.Api;

namespace Yavsc.ApiControllers
{
	public class PaypalApiController: ApiController
	{
		public void GetPayments()
		{
			OAuthTokenCredential tokenCredential =
				new OAuthTokenCredential("<CLIENT_ID>", "<CLIENT_SECRET>");

			string accessToken = tokenCredential.GetAccessToken();
			var parameters = new PayPal.Util.QueryParameters();
			parameters.Add ("Count", "10");

			PaymentHistory paymentHistory = Payment.Get(apiContext, accessToken, parameters);


		}
	}
}

#endif
