using System;
using System.Configuration;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace yavscModel.Blogs
{
	public class BlogEntry	{
		long id;
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

		[StringLength(255)]
		[DisplayName("Nom de l'auteur")]
		public string UserName {
			get {
				return userName;
			}
			set {
				userName = value;
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
		public string [] Tags { get; set ; }
	}
	
}
