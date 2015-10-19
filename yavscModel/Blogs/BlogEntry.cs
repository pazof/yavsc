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
	public class BlogEntry : BasePost	{


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

	}
	
}
