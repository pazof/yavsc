//
//  Notification.cs
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

namespace Yavsc.Model.Google.Api.Messaging
{
	/// <summary>
	/// Notification.
	/// </summary>
	public class Notification {
		/// <summary>
		/// The title.
		/// </summary>
		public string title;
		/// <summary>
		/// The body.
		/// </summary>
		public string body;
		/// <summary>
		/// The icon.
		/// </summary>
		public string icon;
		/// <summary>
		/// The sound.
		/// </summary>
		public string sound;
		/// <summary>
		/// The tag.
		/// </summary>
		public string tag;
		/// <summary>
		/// The color.
		/// </summary>
		public string color;
		/// <summary>
		/// The click action.
		/// </summary>
		public string click_action;

	}
	// https://gcm-http.googleapis.com/gcm/send
	
}
