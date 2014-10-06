using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Blogs
{
	public class BlogEditEntryModel:BlogEntry
	{
		[DisplayName("Prévisualiser")]
		[Required]
		public bool Preview { get; set; }
		public BlogEditEntryModel ()
		{
		}

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

