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
using System.Web.Profile;
using Yavsc.Model.Google;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Configuration;

namespace Yavsc.Model.Google.Api
{
	/// <summary>
	/// Google base API client.
	/// This class implements the identification values for a Google Api, 
	/// and provides some scope values.
	/// </summary>
	public class ApiClient 
	{
		private static string clientId=null;
		private static string clientSecret=null;
		private static string clientApiKey=null;

		/// <summary>
		/// The CLIENT Id.
		/// </summary>
		public static string CLIENT_ID { 
			get { 
				if (clientId==null)
					clientId = ConfigurationManager.AppSettings ["GOOGLE_CLIENT_ID"];
				return clientId;
			}
		}

		/// <summary>
		/// The CLIENt SECREt
		/// </summary>
		public static string  CLIENT_SECRET {get { 
				if (clientSecret==null)
					clientSecret = ConfigurationManager.AppSettings ["GOOGLE_CLIENT_SECRET"];
				return clientSecret;

			}}

		/// <summary>
		/// The API KEY.
		/// </summary>
		public static string  API_KEY {get { 
				if (clientApiKey==null)
					clientApiKey = ConfigurationManager.AppSettings ["GOOGLE_API_KEY"];
				return clientApiKey;

			}}
		/* // to use in descendence
		* 
		protected static string getPeopleUri = "https://www.googleapis.com/plus/v1/people";
		private static string authUri  = "https://accounts.google.com/o/oauth2/auth";
			*/
		/// <summary>
		/// The Map tracks scope .
		/// </summary>
		protected static string scopeTracks = "https://www.googleapis.com/auth/tracks";
		/// <summary>
		/// The calendar scope.
		/// </summary>
		protected static string scopeCalendar = "https://www.googleapis.com/auth/calendar";

		/// <summary>
		/// The scope openid.
		/// </summary>
		protected static string[] scopeOpenid = { 
			"openid",
			"profile",
			"email"
		};
	}
	
}
