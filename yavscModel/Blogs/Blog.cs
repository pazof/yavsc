using System;
using System.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Blog.
	/// </summary>
	public class Blog: ITitle
	{
		string title;
		
		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
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

