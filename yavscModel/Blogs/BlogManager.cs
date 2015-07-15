using System;
using Yavsc.Model.Blogs;
using Yavsc.Model.RolesAndMembers;
using System.Web;
using System.Web.Security;
using Yavsc.Model.Circles;
using System.Web.Mvc;


namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Blog manager.
	/// </summary>
	public static class BlogManager
	{
		/// <summary>
		/// Removes the comment.
		/// </summary>
		/// <returns>The comment.</returns>
		/// <param name="cmtid">Cmtid.</param>
		public static long RemoveComment (long cmtid)
		{
			return Provider.RemoveComment (cmtid);
		}

		/// <summary>
		/// Comment the specified from, postid, content and visible.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="postid">Postid.</param>
		/// <param name="content">Content.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		public static void Comment (string from, long postid, string content, bool visible)
		{
			provider.Comment (from, postid, content);
		}

		static BlogProvider provider;

		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <value>The provider.</value>
		public static BlogProvider Provider {
			get {
				if (provider == null)
					provider = BlogHelper.GetProvider ();
				return provider;
			}
		}

		/// <summary>
		/// Gets the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		public static BlogEntry GetPost (string username, string title)
		{
			return Provider.GetPost (username, title);
		}

		/// <summary>
		/// Gets the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="postid">Postid.</param>
		public static BlogEntry GetPost (long postid)
		{
			return Provider.GetPost (postid);
		}

		/// <summary>
		/// Post the specified username, title, content and visible.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		/// <param name="content">Content.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		/// <param name="cids">sets the circles.</param>
		public static long Post (string username, string title, string content, bool visible, long [] cids)
		{
			return Provider.Post (username, title, content, visible, cids);
		}

		/// <summary>
		/// Updates the post.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="title">Title.</param>
		/// <param name="content">Content.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		/// <param name="cids">sets the circles.</param>
		public static void UpdatePost (long postid, string title, string content, bool visible,long [] cids)
		{
			Provider.UpdatePost (postid, title, content, visible,cids);
		}


		/// <summary>
		/// Finds the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="readersName">Readers name.</param>
		/// <param name="pattern">Pattern.</param>
		/// <param name="searchflags">Searchflags.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		/// <param name="totalRecords">Total records.</param>
		public static BlogEntryCollection FindPost (string readersName, string pattern, FindBlogEntryFlags searchflags, int pageIndex, int pageSize, out int totalRecords)
		{
			return Provider.FindPost (readersName, pattern, searchflags, pageIndex, pageSize, out totalRecords);
		}

		/// <summary>
		/// Removes the post.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		public static void RemovePost (string username, string title)
		{
			if (!Roles.IsUserInRole ("Admin")) {
				string rguser = Membership.GetUser ().UserName;
				if (rguser != username) {
					throw new AccessViolationException (
						string.Format (
							"{1}, Vous n'avez pas le droit de suprimer des billets du Blog de {0}",
							username, rguser));
				}
			}
			Provider.RemovePost (username, title);
		}

		/// <summary>
		/// Lasts the posts.
		/// </summary>
		/// <returns>The posts.</returns>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		/// <param name="totalRecords">Total records.</param>
		public static BlogEntryCollection LastPosts (int pageIndex, int pageSize, out int totalRecords)
		{
			return Provider.LastPosts (pageIndex, pageSize, out totalRecords);
		}

		/// <summary>
		/// Gets the comments.
		/// </summary>
		/// <returns>The comments.</returns>
		/// <param name="postid">Postid.</param>
		/// <param name="getHidden">If set to <c>true</c> get hidden.</param>
		public static Comment[] GetComments (long postid, bool getHidden = true)
		{
			return Provider.GetComments (postid, getHidden);
		}

		/// <summary>
		/// Tag the specified post by postid.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="tag">Tag.</param>
		/// <returns>The tag identifier</returns>
		public static long Tag (long postid, string tag)
		{
			return Provider.Tag (postid, tag);
		}

	}
}

