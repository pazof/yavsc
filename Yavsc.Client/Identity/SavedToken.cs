//
//  SavedToken.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016 GNU GPL
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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Configuration;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.IO;
using Yavsc.Model;
using Yavsc.Client;

namespace Yavsc.Model.Identity
{

	/// <summary>
	/// Saved token.
	/// </summary>
	public class SavedToken  : BaseAuthToken, IIdentified<long> {
		public DateTime expires { get; set; }
		public long Id { get; set; }
	}

}