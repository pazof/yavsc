using System;
using System.Configuration;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Yavsc.Model.Circles;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Blog entry.
	/// </summary>
	public class BlogEntry : BasePost	{
		
		string content;

		/// <summary>
		/// Gets or sets the content.
		/// </summary>
		/// <value>The content.</value>
		[DisplayName("Corps du billet")]	
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
