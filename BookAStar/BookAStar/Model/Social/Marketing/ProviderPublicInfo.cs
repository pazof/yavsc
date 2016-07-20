//
//  ProviderPublicInfo.cs
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


namespace BookAStar.Model.Workflow.Marketing
{
    using Social;
    using System;
    using Yavsc;    /// <summary>
                    /// Provider public info.
                    /// </summary>
    public class ProviderPublicInfo {
		/// <summary>
		/// Gets or sets the display name.
		/// </summary>
		/// <value>The display name.</value>

		public string DisplayName { get; set; }
		/// <summary>
		/// Gets or sets the type of the location.
		/// </summary>
		/// <value>The type of the location.</value>

		public string LocationType { get; set; }
		/// <summary>
		/// Gets or sets the location.
		/// </summary>
		/// <value>The location.</value>

		public Location Location { get; set; }
		/// <summary>
		/// Gets or sets the logo image locator.
		/// </summary>
		/// <value>The logo image locator.</value>
		public string LogoImgLocator { get; set; }
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>

		public string Description { get; set;}
		/// <summary>
		/// Gets or sets the web page.
		/// </summary>
		/// <value>The web page.</value>
		public string WebPage { get; set; }
	}
	
}
