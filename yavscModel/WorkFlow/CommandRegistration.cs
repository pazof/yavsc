//
//  CommandRegistration.cs
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
using Yavsc.Model.WorkFlow;
using System.Configuration;
using System.Collections.Specialized;
using Yavsc.Model.FrontOffice;
using System.Configuration.Provider;
using Yavsc.Model.FrontOffice.Catalog;
using System.Collections.Generic;
using Yavsc.Model.Skill;
using System.Linq;
using Yavsc.Model.Calendar;
using Yavsc.Model.Google.Api;
using System.Net.Mail;
using System.Web.Security;
using System.Web.Configuration;
using System.Net;
using System.IO;

namespace Yavsc.Model.WorkFlow
{
	public class CommandRegistration {
		public long CommandId {get; set; }
	}
	
}
