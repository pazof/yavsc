//
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
using Yavsc.Helpers;
using Yavsc.Models.Google;

namespace Yavsc.GoogleApis
{
    /// <summary>
    /// Google Map tracks Api client.
    /// </summary>
    public class MapTracks {
        protected static string scopeTracks = "https://www.googleapis.com/auth/tracks";
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Helpers.Google.Api.MapTracks"/> class.
		/// </summary>
		/// <param name="authType">Auth type.</param>
		/// <param name="redirectUri">Redirect URI.</param>
		public MapTracks()
		{}
		/// <summary>
		/// The google map tracks path (uri of the service).
		/// </summary>
		protected static string googleMapTracksPath = "https://www.googleapis.com/tracks/v1/";
		// entities/[create|list|delete]
		// collections/[list|create|[add|remove]entities|delete]
		// crumbs/[record|getrecent|gethistory|report|summarize|getlocationinfo|delete


		// entities/[create|list|delete]
		// collections/[list|create|[add|remove]entities|delete]
		// crumbs/[record|getrecent|gethistory|report|summarize|getlocationinfo|delete

		/// <summary>
		/// Creates the entity.
		/// </summary>
		/// <returns>The entity.</returns>
		/// <param name="entities">Entities.</param>
		public static string [] CreateEntity( Entity[] entities ) {
			string [] ans = null;
            
			using (SimpleJsonPostMethod wr =
				new SimpleJsonPostMethod (googleMapTracksPath + "entities/create"))
			{
				ans =  wr.Invoke<string[]> (entities);
			}
			return ans;
		}

		/// <summary>
		/// Lists the entities.
		/// </summary>
		/// <returns>The entities.</returns>
		/// <param name="eq">Eq.</param>
		static Entity[] ListEntities (EntityQuery eq)
		{
			Entity [] ans = null;
			using (SimpleJsonPostMethod wr =
				new SimpleJsonPostMethod (googleMapTracksPath + "entities/create"))
				{
					ans = wr.Invoke <Entity[]> (eq);
				}
				return ans;
		}
	}
}
