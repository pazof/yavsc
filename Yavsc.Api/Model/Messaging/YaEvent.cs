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

using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Messaging
{
	/// <summary>
	/// NF event.
	/// </summary>
	public class YaEvent : BaseEvent
	{
		/// <summary>
		/// The name of the NF provider.
		/// </summary>
		[Display(Name="ProviderName")]
		public string ProviderName { get; set; } 
		/// <summary>
		/// The NF provider identifier.
		/// </summary>
		[Display(Name="ProviderId")]
		public string ProviderId { get; set; }
		/// <summary>
		/// The promotion code.
		/// </summary>
		[Display(Name="Comment")]
		public string Comment { get; set; }
		/// <summary>
		/// The event web page.
		/// </summary>
		[Display(Name="EventWebPage")]
		public string EventWebPage { get; set; }
		/// <summary>
		/// The image locator.
		/// </summary>
		[Display(Name="Photo")]
		public string Photo { get; set; }
	}
}
