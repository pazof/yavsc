using System;
using System.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Yavsc.Model.Blogs
{
	public class Blog
	{
		string title;

		[StringLength(512)]
		[Required]
		[DisplayName("Titre")]
		public string Title {
			get {
				return title;
			}
			set {
				title = value;
			}
		}

	}
}

