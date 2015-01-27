﻿//
//  Google.cs
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

	public class MapTracks:ApiClient {
		protected static string googleMapTracksPath = "https://www.googleapis.com/tracks/v1/";
		// entities/[create|list|delete]
		// collections/[list|create|[add|remove]entities|delete]
		// crumbs/[record|getrecent|gethistory|report|summarize|getlocationinfo|delete


		// entities/[create|list|delete]
		// collections/[list|create|[add|remove]entities|delete]
		// crumbs/[record|getrecent|gethistory|report|summarize|getlocationinfo|delete


		static string [] CreateEntity( Entity[] entities ) {
			string [] ans = null;
			using (SimpleJsonPostMethod< Entity[] ,string []> wr = 
				new SimpleJsonPostMethod< Entity[] ,string[]> (googleMapTracksPath + "entities/create")) 
			{
				ans = wr.Invoke (entities);
			}
			return ans;
		}
		static Entity[] ListEntities (EntityQuery eq)
		{
			Entity [] ans = null;
			using (SimpleJsonPostMethod<EntityQuery,Entity[]> wr = 
				new SimpleJsonPostMethod<EntityQuery,Entity[]> (googleMapTracksPath + "entities/create")) 
				{ 
					ans = wr.Invoke (eq);
				}
				return ans;
		}
	}
}