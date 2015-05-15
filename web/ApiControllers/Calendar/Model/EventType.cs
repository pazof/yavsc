//
//  EventType.cs
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

namespace Yavsc.ApiControllers.Calendar.Model
{
	/// <summary>
	/// Event type.
	/// </summary>
	public enum EventType
	{
		/// <summary>
		/// The concert gratuit.
		/// </summary>
		ConcertGratuit,
		/// <summary>
		/// The concert prive.
		/// </summary>
		ConcertPrive,
		/// <summary>
		/// The distraciton.
		/// </summary>
		Distraciton,
		/// <summary>
		/// The rencontre.
		/// </summary>
		Rencontre,
		/// <summary>
		/// The assemblee.
		/// </summary>
		Assemblee,
		/// <summary>
		/// The reunion.
		/// </summary>
		Reunion,
		/// <summary>
		/// The bureau.
		/// </summary>
		Bureau,
		/// <summary>
		/// The manifestation.
		/// </summary>
		Manifestation,
		/// <summary>
		/// The zone de danger.
		/// </summary>
		ZoneDeDanger
	}
	
}
