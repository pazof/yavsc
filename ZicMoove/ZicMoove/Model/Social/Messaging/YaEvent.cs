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

namespace BookAStar.Model.Workflow.Messaging
{
	/// <summary>
	/// NF event.
	/// </summary>
	public class YaEvent : BaseEvent
	{
		/// <summary>
		/// The name of the NF provider.
		/// </summary>
		public string ProviderName { get; set; } 
		/// <summary>
		/// The NF provider identifier.
		/// </summary>
		public string ProviderId { get; set; }
		/// <summary>
		/// The promotion code.
		/// </summary>
		public string Comment { get; set; }
		/// <summary>
		/// The event web page.
		/// </summary>
		public string EventWebPage { get; set; }
		/// <summary>
		/// The image locator.
		/// </summary>
		public string Photo { get; set; }
	}
}
