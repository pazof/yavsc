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
using System.Configuration;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.IO;
using Yavsc.Model;

namespace Yavsc.Model.Google
{
	/// <summary>
	/// People.
	/// </summary>
	public class People {
		/// <summary>
		/// Gets or sets the kind.
		/// </summary>
		/// <value>The kind.</value>
		public string kind { get; set; }
		/// <summary>
		/// Gets or sets the etag.
		/// </summary>
		/// <value>The etag.</value>
		public string etag { get; set; }
		/// <summary>
		/// Gets or sets the gender.
		/// </summary>
		/// <value>The gender.</value>
		public string gender { get; set; }
		/// <summary>
		/// E mail.
		/// </summary>
		public class EMail{ 
			/// <summary>
			/// Gets or sets the value.
			/// </summary>
			/// <value>The value.</value>
			public string value { get; set; }
			/// <summary>
			/// Gets or sets the type.
			/// </summary>
			/// <value>The type.</value>
			public string type { get; set; }
		}
		/// <summary>
		/// Gets or sets the emails.
		/// </summary>
		/// <value>The emails.</value>
		public EMail[] emails { get; set; }
		/// <summary>
		/// Gets or sets the type of the object.
		/// </summary>
		/// <value>The type of the object.</value>
		public string objectType { get; set; }
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string id { get; set; }
		/// <summary>
		/// Gets or sets the display name.
		/// </summary>
		/// <value>The display name.</value>
		public string displayName { get; set; }
		/// <summary>
		/// Name.
		/// </summary>
		public class Name {
			/// <summary>
			/// Gets or sets the name of the family.
			/// </summary>
			/// <value>The name of the family.</value>
			public string familyName { get; set; }
			/// <summary>
			/// Gets or sets the name of the given.
			/// </summary>
			/// <value>The name of the given.</value>
			public string givenName { get; set; }
		}
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public Name name { get; set;}
		/// <summary>
		/// Gets or sets the URL.
		/// </summary>
		/// <value>The URL.</value>
		public string url { get; set; }
		/// <summary>
		/// Image.
		/// </summary>
		public class Image {
			/// <summary>
			/// Gets or sets the URL.
			/// </summary>
			/// <value>The URL.</value>
			public string url { get; set; }
			/// <summary>
			/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Google.People.Image"/> is default.
			/// </summary>
			/// <value><c>true</c> if is default; otherwise, <c>false</c>.</value>
			public bool isDefault { get; set; }
		}
		/// <summary>
		/// Gets or sets the image.
		/// </summary>
		/// <value>The image.</value>
		public Image image { get; set; }
		/// <summary>
		/// Place.
		/// </summary>
		public class Place {
			/// <summary>
			/// Gets or sets the value.
			/// </summary>
			/// <value>The value.</value>
			public string value { get; set; }
			/// <summary>
			/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Google.People.Place"/> is primary.
			/// </summary>
			/// <value><c>true</c> if primary; otherwise, <c>false</c>.</value>
			public bool primary { get; set; }
		}
		/// <summary>
		/// Gets or sets the places lived.
		/// </summary>
		/// <value>The places lived.</value>
		public Place[] placesLived { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Google.People"/> is plus user.
		/// </summary>
		/// <value><c>true</c> if is plus user; otherwise, <c>false</c>.</value>
		public bool isPlusUser { get; set; }
		/// <summary>
		/// Gets or sets the language.
		/// </summary>
		/// <value>The language.</value>
		public string language { get; set; }
		/// <summary>
		/// Gets or sets the circled by count.
		/// </summary>
		/// <value>The circled by count.</value>
		public int circledByCount { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Google.People"/> is verified.
		/// </summary>
		/// <value><c>true</c> if verified; otherwise, <c>false</c>.</value>
		public bool verified { get; set; }
	}
}
