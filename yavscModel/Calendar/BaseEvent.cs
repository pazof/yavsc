//
//  BaseEvent.cs
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
using System.Web.Http;
using System.ComponentModel.DataAnnotations;
using Yavsc.Model;

namespace Yavsc.Model.Calendar
{
	/// <summary>
	/// Base event.
	/// </summary>
	public class BaseEvent: ITitle
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
	
}
