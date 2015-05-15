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

namespace Yavsc.ApiControllers.Calendar.Model
{

	/// <summary>
	/// NF event.
	/// </summary>
	public class YaEvent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.ApiControllers.Calendar.Model.YaEvent"/> class.
		/// </summary>
		public YaEvent()
		{
			this.EstabType = EstablishmentType.DomainePublique;
			this.NFEvType = EventType.Distraciton;
		}

		/// <summary>
		/// Gets or sets the type of the NF ev.
		/// </summary>
		/// <value>The type of the NF ev.</value>
		[Required]
		public EventType NFEvType { get; set; }

		/// <summary>
		/// Gets or sets the type of the estab.
		/// </summary>
		/// <value>The type of the estab.</value>
		[Required(ErrorMessage="Please, choose an establisment type.")]
		public EstablishmentType EstabType { get; set; }

		/// <summary>
		/// The title.
		/// </summary>
		[Required(ErrorMessage="Please, choose a .")] 
		public string Title { get; set; }
		/// <summary>
		/// The description.
		/// </summary>
		[Required(ErrorMessage="Please, choose a Description.")] public string Description { get; set; }
		/// <summary>
		/// The location.
		/// </summary>
		[Required(ErrorMessage="Please, choose a Location.")] public Position Location { get; set; }
		/// <summary>
		/// The start date.
		/// </summary>
		[Required(ErrorMessage="Please, choose a Start Date.")] public DateTime StartDate { get; set; }

		/// <summary>
		/// Gets or sets the end date.
		/// </summary>
		/// <value>The end date.</value>
		[Required(ErrorMessage="Please, choose an End Date.")] public DateTime EndDate { get; set; }
		/// <summary>
		/// The name of the NF provider.
		/// </summary>
		[Required(ErrorMessage="Please, choose a Nigth Flash Provider Name.")] public string NFProviderName { get; set; }
		/// <summary>
		/// The NF provider identifier.
		/// </summary>
		[Required(ErrorMessage="Please, choose a Night Flash Provider Identifier.")] public string NFProviderId { get; set; }
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
		/// </summary>-
		public string ImgLocator { get; set; }
	}
}
