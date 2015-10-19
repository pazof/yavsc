//
//  AllTests.cs
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

using NUnit.Framework;
using System;
using Yavsc.Model.Blogs;
using Yavsc.Controllers;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Configuration;
using System.Configuration;
using System.IO;
using System.Web.Http;
using Mono.WebServer;
using System.Net;
using System.Collections;

namespace Yavsc
{
	public class AllTests
	{
		[Suite]
		public static IEnumerable Suite
		{
			get
			{
				ArrayList suite = new ArrayList ();
				suite.Add(new BlogUnitTestCase());
				return suite;
			}
		}
	}
	
}
