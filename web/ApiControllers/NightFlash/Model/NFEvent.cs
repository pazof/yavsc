//
//  NFEvent.cs
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

namespace Yavsc.ApiControllers.NightFlash.Model
{

	/// <summary>
	/// NF event.
	/// </summary>
	public class NFEvent
	{
		/// <summary>
		/// The title.
		/// </summary>
		[Required] public string Title { get; set; }
		/// <summary>
		/// The description.
		/// </summary>
		[Required] public string Description { get; set; }
		/// <summary>
		/// The type of the event.
		/// </summary>
		[Required] public string EventType { get; set; }
		/// <summary>
		/// The location.
		/// </summary>
		[Required] public Position Location { get; set; }
		/// <summary>
		/// The start date.
		/// </summary>
		[Required] public DateTime StartDate { get; set; }
		/// <summary>
		/// The name of the NF provider.
		/// </summary>
		[Required] public string NFProviderName { get; set; }
		/// <summary>
		/// The NF provider identifier.
		/// </summary>
		[Required] public string NFProviderId { get; set; }
		/// <summary>
		/// The type of the location.
		/// </summary>
		[Required] public string LocationType { get; set; }
		/// <summary>
		/// The promotion code.
		/// </summary>
		public string PromotionCode { get; set; }
		/// <summary>
		/// The event web page.
		/// </summary>
		public string EventWebPage { get; set; }
		/// <summary>
		/// The image locator.
		/// </summary>
		public string ImgLocator { get; set; }
	}
	
}
