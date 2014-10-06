using System;
using System.Configuration;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Blogs
{
	public class Comment
	{
		long id;
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

		public DateTime posted;

		[DisplayName("Date de creation")]
		public DateTime Posted {
			get {
				return posted;
			}
			set {
				posted = value;
			}
		}

		public DateTime modified;

		[DisplayName("Date de modification")]
		public DateTime Modified {
			get {
				return modified;
			}
			set {
				modified = value;
			}
		}

		public bool Visible { get; set ; }

	}
}

