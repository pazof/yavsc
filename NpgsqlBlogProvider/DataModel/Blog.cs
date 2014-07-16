using System;
using System.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Npgsql.Web.Blog.DataModel
{
	public class Blog
	{
		string title;

		[StringValidator(MaxLength=512)]
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

