//
//  People.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2014 Paul Schneider
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
using System.Web.Mvc;
using System.Configuration;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.IO;
using Yavsc.Model;

namespace Yavsc.Model.Google
{
	public class People {
		public string kind { get; set; }
		public string etag { get; set; }
		public string gender { get; set; }
		public class EMail{ 
			public string value { get; set; }
			public string type { get; set; }
		}
		public EMail[] emails { get; set; }
		public string objectType { get; set; }
		public string id { get; set; }
		public string displayName { get; set; }
		public class Name {
			public string familyName { get; set; }
			public string givenName { get; set; }
		}
		public Name name { get; set;}
		public string url { get; set; }
		public class Image {
			public string url { get; set; }
			public bool isDefault { get; set; }
		}
		public Image image { get; set; }
		public class Place {
			public string value { get; set; }
			public bool primary { get; set; }
		}
		public Place[] placesLived { get; set; }
		public bool isPlusUser { get; set; }
		public string language { get; set; }
		public int circledByCount { get; set; }
		public bool verified { get; set; }
	}
}
