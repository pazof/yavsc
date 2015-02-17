using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Blog edit entry model.
	/// </summary>
	public class BlogEditEntryModel:BlogEntry
	{
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Blogs.BlogEditEntryModel"/> is preview.
		/// </summary>
		/// <value><c>true</c> if preview; otherwise, <c>false</c>.</value>
		[DisplayName("Prévisualiser")]
		[Required]
		public bool Preview { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Blogs.BlogEditEntryModel"/> class.
		/// </summary>
		public BlogEditEntryModel ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Blogs.BlogEditEntryModel"/> class.
		/// </summary>
		/// <param name="be">Be.</param>
		public BlogEditEntryModel(BlogEntry be) {
			this.Preview = true;
			this.Content = be.Content;
			this.Title = be.Title;
			this.Posted = be.Posted;
			this.Modified = be.Modified;
			this.Visible = be.Visible;
			this.UserName = be.UserName;
			this.Id = be.Id;
		}
	}
}

