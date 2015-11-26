//
//  MessageWithPayLoad.cs
//
//  Author:
//       paul <>
//
//  Copyright (c) 2015 paul
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
using System.Web.Mvc;

namespace Yavsc.Model.Google
{
	// https://gcm-http.googleapis.com/gcm/send
	/// <summary>
	/// Message with payload.
	/// </summary>
	public class MessageWithPayload<T> {
		/// <summary>
		/// To.
		/// </summary>
		public string to;
		/// <summary>
		/// The registration identifiers.
		/// </summary>
		public string [] registration_ids; 
		/// <summary>
		/// The data.
		/// </summary>
		public T[] data ;
		/// <summary>
		/// The notification.
		/// </summary>
		public Notification notification;
		/// <summary>
		/// The collapse key.
		/// </summary>
		public string collapse_key; // in order to collapse ...
		/// <summary>
		/// The priority.
		/// </summary>
		public int priority; // between 0 and 10, 10 is the lowest!
		/// <summary>
		/// The content available.
		/// </summary>
		public bool content_available; 
		/// <summary>
		/// The delay while idle.
		/// </summary>
		public bool delay_while_idle; 
		/// <summary>
		/// The time to live.
		/// </summary>
		public int time_to_live; // seconds
		/// <summary>
		/// The name of the restricted package.
		/// </summary>
		public string restricted_package_name;
		/// <summary>
		/// The dry run.
		/// </summary>
		public bool dry_run;
		/// <summary>
		/// Validate the specified modelState.
		/// </summary>
		/// <param name="modelState">Model state.</param>
		public void Validate(ModelStateDictionary modelState) {
			if (to==null && registration_ids == null) {
				modelState.AddModelError ("to", "One of \"to\" or \"registration_ids\" parameters must be specified");
				modelState.AddModelError ("registration_ids", "*");
				modelState.AddModelError ("to", "*");
			}
			if (notification == null && data == null) {
				modelState.AddModelError ("notification", "At least one of \"notification\" or \"data\" parameters must be specified");
				modelState.AddModelError ("data", "*");
			}
			if (notification != null) {
				if (notification.icon == null)
					modelState.AddModelError ("notification.icon", "please, specify an icon resoure name");
				if (notification.title == null)
					modelState.AddModelError ("notification.title", "please, specify a title");
			}
		}
	}
}

