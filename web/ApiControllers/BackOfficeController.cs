//
//  BackOfficeController.cs
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
using Yavsc.Model.Google;
using Yavsc.Model.Calendar;
using Yavsc.Helpers;
using Yavsc.Model.Circles;
using System.Collections.Generic;
using System.Web.Profile;
using Yavsc.Helpers.Google;
using System.Web.Security;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Back office controller.
	/// </summary>
	public class BackOfficeController: YavscController
	{
		/// <summary>
		/// Notifies the event.
		/// </summary>
		/// <param name="evpub">Evpub.</param>
		[Authorize(Roles="BackOffice")]
		public MessageWithPayloadResponse NotifyEvent(EventPub evpub) {
			return GoogleHelpers.NotifyEvent(evpub);
		}

		/// <summary>
		/// Sets the registration identifier.
		/// </summary>
		/// <param name="registrationId">Registration identifier.</param>
		[Authorize]
		public void SetRegistrationId(string registrationId)
		{
			// TODO set registration id
			setRegistrationId (User.Identity.Name, registrationId);
		}

		private void setRegistrationId(string username, string regid) {
			ProfileBase pr = ProfileBase.Create(username);
			pr.SetPropertyValue ("gregid", regid);
			pr.Save ();
		}

	}
}

