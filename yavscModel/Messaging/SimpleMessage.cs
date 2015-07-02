//
//  Message.cs
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

namespace Yavsc.Model.Messaging
{
	/// <summary>
	/// Simple message.
	/// </summary>
	public class SimpleMessage
	{
		/// <summary>
		/// Gets or sets the user name this message is comming from.
		/// </summary>
		/// <value>From.</value>
		public string From { get; set; }
		/// <summary>
		/// Gets or sets the user names, separted by semilicon to which this message will be sent.
		/// </summary>
		/// <value>To.</value>
		public string To { get; set; }
		/// <summary>
		/// Gets or sets the subject.
		/// </summary>
		/// <value>The subject.</value>
		public string Subject { get; set; }
		/// <summary>
		/// Gets or sets the body.
		/// </summary>
		/// <value>The body.</value>
		public string Body { get; set; }
	}
}

