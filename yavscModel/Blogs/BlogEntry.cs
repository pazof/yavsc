using System;
using System.Configuration;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Yavsc.Model.Circles;
using System.Web.Mvc;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Blog entry.
	/// </summary>
	public class BlogEntry	{
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

		string title;

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

		string content;

		/// <summary>
		/// Gets or sets the content.
		/// </summary>
		/// <value>The content.</value>
		[DisplayName("Corps du billet")]	
		[Required(ErrorMessage = "S'il vous plait, saisissez un texte.")]
		public string Content {
			get {
				return content;
			}
			set {
				content = value;
			}
		}

		string userName;

		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		[StringLength(255)]
		[DisplayName("Auteur")]
		public string Author {
			get {
				return userName;
			}
			set {
				userName = value;
			}
		}
		/// <summary>
		/// The posted.
		/// </summary>
		public DateTime posted;

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
		public DateTime modified;

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

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Blogs.BlogEntry"/> is visible.
		/// </summary>
		/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
		public bool Visible { get; set ; }

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
