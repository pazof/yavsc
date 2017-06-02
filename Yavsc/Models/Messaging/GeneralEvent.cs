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

using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Messaging
{
public class GeneralEvent: BaseEvent, ITitle
	{
		/// <summary>
		/// The title.
		/// </summary>
		[Required(ErrorMessageResourceName="ChooseATitle")]
		[Display(Name="Title")]
		public string Title { get; set; }
		/// <summary>
		/// The description.
		/// </summary>
		[Required(ErrorMessageResourceName="ChooseADescription")]
		[Display(Name="Description")]
		public string Description { get; set; }

	}

}
