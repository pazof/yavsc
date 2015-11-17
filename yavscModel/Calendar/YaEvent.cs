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
using Yavsc.Model;

namespace Yavsc.Model.Calendar
{
	/// <summary>
	/// Base event.
	/// </summary>
	public class BaseEvent
	{
		/// <summary>
		/// The title.
		/// </summary>
		[Required(ErrorMessage="Please, choose a .")] 
		[Display(ResourceType=typeof(LocalizedText),Name="Title")]
		public string Title { get; set; }
		/// <summary>
		/// The description.
		/// </summary>
		[Required(ErrorMessage="Please, choose a Description.")] 
		[Display(ResourceType=typeof(LocalizedText),Name="Description")]
		public string Description { get; set; }

		/// <summary>
		/// The location.
		/// </summary>
		[Required(ErrorMessage="Please, choose a Location.")] 
		[Display(ResourceType=typeof(LocalizedText),Name="Location")]
		public Position Location { get; set; }
		/// <summary>
		/// The start date.
		/// </summary>
		[Required(ErrorMessage="Please, choose a Start Date.")] 
		[Display(ResourceType=typeof(LocalizedText),Name="StartDate")]
		public DateTime StartDate { get; set; }

		/// <summary>
		/// Gets or sets the end date.
		/// </summary>
		/// <value>The end date.</value>
		[Required(ErrorMessage="Please, choose an End Date.")] 
		[Display(ResourceType=typeof(LocalizedText),Name="EndDate")]
		public DateTime EndDate { get; set; }

	}
	/// <summary>
	/// NF event.
	/// </summary>
	public class YaEvent : BaseEvent
	{
		/// <summary>
		/// The name of the NF provider.
		/// </summary>
		[Required(ErrorMessage="Please, choose a Provider Name.")]
		[Display(ResourceType=typeof(LocalizedText),Name="ProviderName")]
		public string ProviderName { get; set; }
		/// <summary>
		/// The NF provider identifier.
		/// </summary>
		[Required(ErrorMessage="Please, choose a Provider Identifier.")]
		[Display(ResourceType=typeof(LocalizedText),Name="ProviderId")]
		public string ProviderId { get; set; }
		/// <summary>
		/// The promotion code.
		/// </summary>
		[Display(ResourceType=typeof(LocalizedText),Name="Comment")]
		public string Comment { get; set; }
		/// <summary>
		/// The event web page.
		/// </summary>
		[Display(ResourceType=typeof(LocalizedText),Name="EventWebPage")]
		public string EventWebPage { get; set; }
		/// <summary>
		/// The image locator.
		/// </summary>
		[Display(ResourceType=typeof(LocalizedText),Name="Photo")]
		public string Photo { get; set; }
	}
}
