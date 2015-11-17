//
//  BasePost.cs
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
using System.Configuration;
using System.Collections.Generic;
using Yavsc.Model.Blogs;
using System.Linq;
using Yavsc.Model.Circles;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Base post.
	/// </summary>
	public class BasePost: IRating {
		/// <summary>
		/// The identifier.
		/// </summary>

		long id;
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		[DisplayName("Identifiant numérique de billet")]
		public long Id {
			get {
				return id;
			}
			set {
				id = value;
			}
		}

		/// <summary>
		/// The posted.
		/// </summary>
		private DateTime posted;

		/// <summary>
		/// Gets or sets the posted.
		/// </summary>
		/// <value>The posted.</value>
		[DisplayName("Date de creation")]
		public DateTime Posted {
			get {
				return posted;
			}
			set {
				posted = value;
			}
		}
		/// <summary>
		/// The modified.
		/// </summary>
		private DateTime modified;

		/// <summary>
		/// Gets or sets the modified.
		/// </summary>
		/// <value>The modified.</value>
		[DisplayName("Date de modification")]
		public DateTime Modified {
			get {
				return modified;
			}
			set {
				modified = value;
			}
		}

		private string title;

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		[DisplayName("Titre du billet")]
		[StringLength(512)]
		[RegularExpression("^[^:%&?]*$",ErrorMessage = "Les caratères suivants sont invalides pour un titre: :%&?")]
		[Required(ErrorMessage = "S'il vous plait, saisissez un titre")]
		public string Title {
			get {
				return title;
			}
			set {
				title = value;
			}
		}

		private string author;

		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		[StringLength(255)]
		[DisplayName("Auteur")]
		public string Author {
			get {
				return author;
			}
			set {
				author = value;
			}
		}

		/// <summary>
		/// Gets or sets the photo.
		/// </summary>
		/// <value>The photo.</value>
		public string Photo { 
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Blogs.BlogEntry"/> is visible.
		/// </summary>
		/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
		public bool Visible { get; set ; }

		/// <summary>
		/// Gets or sets the rate.
		/// </summary>
		/// <value>The rate.</value>
		public int Rate { get; set; }

		/// <summary>
		/// Gets or sets the circles allowed to read this ticket.
		/// An empty collection specifies a public post.
		/// </summary>
		/// <value>The circles.</value>
		[Display(Name="Cercles autorisés")]
		public long[] AllowedCircles { get; set; }

		/// <summary>
		/// Gets or sets the tags.
		/// </summary>
		/// <value>The tags.</value>
		public string [] Tags { get; set ; }
	}
}
