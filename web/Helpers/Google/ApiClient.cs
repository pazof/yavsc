//
//  Manager.cs
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
using Yavsc.Helpers;
using System.Web.Profile;
using Yavsc.Model.Google;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Yavsc.Helpers.Google
{

	public class ApiClient 
	{
		protected static string CLIENT_ID = "325408689282-6bekh7p3guj4k0f3301a6frf025cnrk1.apps.googleusercontent.com";
		protected static string CLIENT_SECRET = "MaxYcvJJCs2gDGvaELZbzwfL";
		protected static string API_KEY="AIzaSyBV_LQHb22nGgjNvFzZwnQHjao3Q7IewRw";
		/* // to use in descendence
		* 
		protected static string getPeopleUri = "https://www.googleapis.com/plus/v1/people";
		private static string authUri  = "https://accounts.google.com/o/oauth2/auth";
			*/

		protected static string scopeTracks = "https://www.googleapis.com/auth/tracks";
		protected static string scopeCalendar = "https://www.googleapis.com/auth/calendar";
		protected static string[] scopeOpenid = { 
			"openid",
			"profile",
			"email"
		};

		// private static string dateFormat = "yyyy-MM-ddTHH:mm:ss";


	}
	
}
