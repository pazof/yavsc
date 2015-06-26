//
//  EventPub.cs
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
using System.ComponentModel.DataAnnotations;
using Yavsc.Model;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Model.Circles;

namespace Yavsc.Model.Calendar
{
	/// <summary>
	/// Event pub.
	/// </summary>
	public class EventPub: YaEvent
	{
		/// <summary>
		/// Gets or sets the circles.
		/// </summary>
		/// <value>The circles.</value>
		[Display(ResourceType=typeof(LocalizedText),Name="Circles")]
		public Circle[] Circles { get; set; }
	}

}


