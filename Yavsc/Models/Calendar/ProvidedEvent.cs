//
//  ProvidedEvent.cs
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
using Yavsc.Models.Messaging;
using Yavsc.Models.Access;

namespace Yavsc.Models.Calendar
{

	/// <summary>
	/// Provided event.
	/// </summary>
	public class ProvidedEvent : BaseEvent {
		/// <summary>
		/// The privacy.
		/// </summary>
		[Required]
		public Publishing Privacy;

        public override string CreateBody()
        {
            throw new System.NotImplementedException();
        }
    }

}
