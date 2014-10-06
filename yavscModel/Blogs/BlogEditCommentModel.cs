using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Blogs
{
	public class BlogEditCommentModel:Comment
	{
		[DisplayName("Prévisualiser")]
		[Required]
		public bool Preview { get; set; }
		/* TODO Clean
		public BlogEditCommentModel(Comment be) {
			this.Preview = true;
			this.Content = be.Content;
			this.Posted = be.Posted;
			this.Modified = be.Modified;
			this.Visible = be.Visible;
			this.From = be.From;
			this.PostId = be.PostId;
			this.Id = be.Id;
		} */
	}
}

