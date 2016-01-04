//
//  NominativeEventPub.cs
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
using System.ComponentModel.DataAnnotations;
using Yavsc.Model;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Model.Circles;
using Yavsc.Model.FrontOffice;

namespace Yavsc.Model.Calendar
{

	public class NominativeEventPub: YaEvent, INominative
	{
		#region INominative implementation
		public string PerformerName {
			get ; set ;
		}
		#endregion
	}
}
