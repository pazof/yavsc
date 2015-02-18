//
//  CommandStatus.cs
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

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Command status.
	/// </summary>
	public enum CommandStatus:int
	{
		/// <summary>
		/// The inserted.
		/// </summary>
		Inserted,
		/// <summary>
		/// The user validated.
		/// </summary>
		UserValidated,
		/// <summary>
		/// The user canceled.
		/// </summary>
		UserCanceled,
		/// <summary>
		/// The execution pending.
		/// </summary>
		ExecutionPending,
		/// <summary>
		/// The satisfied.
		/// </summary>
		Satisfied,
		/// <summary>
		/// The refunded.
		/// </summary>
		Refunded
	}
}

