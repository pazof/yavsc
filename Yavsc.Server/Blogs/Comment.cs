using System;
using System.Configuration;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Comment.
	/// </summary>
	public class Comment
	{
		long id;

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		[DisplayName("Identifiant numérique de commentaire")]
		public long Id {
			get {
				return id;
			}
			set {
				id = value;
			}
		}
		long postid;

		/// <summary>
		/// Gets or sets the post identifier.
		/// </summary>
		/// <value>The post identifier.</value>
		[DisplayName("Identifiant numérique du billet commenté")]
		public long PostId {
			get {
				return postid;
			}
			set {
				postid = value;
			}
		}

		/// <summary>
		/// Gets or sets the author of this comment.
		/// </summary>
		/// <value>From.</value>
		public string From { get; set; }
		
		string content;

		/// <summary>
		/// Gets or sets the comment text.
		/// </summary>
		/// <value>The comment text.</value>
		[DisplayName("Contenu")]	
		[Required(ErrorMessage = "S'il vous plait, saisissez un contenu")]
		public string CommentText {
			get {
				return content;
			}
			set {
				content = value;
			}
		}

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

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Blogs.Comment"/> is visible.
		/// </summary>
		/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
		public bool Visible { get; set ; }

	}
}

