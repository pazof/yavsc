using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Blog edit comment model.
	/// </summary>
	public class BlogEditCommentModel:Comment
	{
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Blogs.BlogEditCommentModel"/> is preview.
		/// </summary>
		/// <value><c>true</c> if preview; otherwise, <c>false</c>.</value>
		[DisplayName("Prévisualiser")]
		[Required]
		public bool Preview { get; set; }

	}
}

