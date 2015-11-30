//
//  MessageWithPayloadResponse.cs
//
//  Author:
//       paul <paul@pschneider.fr>
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
	// https://gcm-http.googleapis.com/gcm/send

	/// <summary>
	/// Message with payload response.
	/// </summary>
	public class MessageWithPayloadResponse { 
		/// <summary>
		/// The multicast identifier.
		/// </summary>
		public int multicast_id;
		/// <summary>
		/// The success count.
		/// </summary>
		public int success;
		/// <summary>
		/// The failure count.
		/// </summary>
		public int failure;
		/// <summary>
		/// The canonical identifiers... ?!?
		/// </summary>
		public int canonical_ids;
		/// <summary>
		/// Detailled result.
		/// </summary>
		public class Result {
			/// <summary>
			/// The message identifier.
			/// </summary>
			public string message_id;
			/// <summary>
			/// The registration identifier.
			/// </summary>
			public string registration_id;
			/// <summary>
			/// The error.
			/// </summary>
			public string error;
		}

		/// <summary>
		/// The results.
		/// </summary>
		public Result [] results;
	}
}
