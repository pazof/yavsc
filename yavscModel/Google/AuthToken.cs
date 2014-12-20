//
//  GoogleAuthToken.cs
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
		/*
 "url": "https://plus.google.com/111395572362177872801",
 "image": {
  "url": "https://lh6.googleusercontent.com/-JqDVMPqafdA/AAAAAAAAAAI/AAAAAAAAADY/FamseW6_nl4/photo.jpg?sz=50",
  "isDefault": false
 },
 "placesLived": [
  {
   "value": "Suresnes, France",
   "primary": true
  }
 ],
 "isPlusUser": true,
 "language": "fr",
 "circledByCount": 0,
 "verified": false
}
*/

	public class AuthToken {
		public string access_token { get; set; }
		public string id_token { get; set; }
		public int expires_in { get; set; }
		public string token_type { get; set ; }
		public string refresh_token { get; set; }
	}
	
}
