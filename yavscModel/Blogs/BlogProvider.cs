using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Generic;
using Yavsc.Model.Circles;
using System.Web.Mvc;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Blog provider.
	/// </summary>
	public abstract class BlogProvider: ProviderBase
	{
		/// <summary>
		/// Gets the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="postid">Postid.</param>
		public abstract BlogEntry GetPost (long postid);

		/// <summary>
		/// Gets the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		public abstract BlogEntry GetPost (string username, string title);

		/// <summary>
		/// Post the specified username, title, content, visible and allowedCircles.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		/// <param name="content">Content.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		/// <param name="allowedCircles">Allowed circles.</param>
		public abstract long Post (string username, string title, string content, bool visible, long[] allowedCircles);


		/// <summary>
		/// Updates the post.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="title">Title.</param>
		/// <param name="content">Content.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		/// <param name="allowedCircles">Allowed circles.</param>
		public abstract void UpdatePost (long postid, string title, string content, bool visible, long[] allowedCircles);


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
		public abstract BlogEntryCollection FindPost (string readersName, string pattern, FindBlogEntryFlags searchflags, 
			int pageIndex, int pageSize, out int totalRecords);

		/// <summary>
		/// Removes the post.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		public abstract void RemovePost (string username, string title);

		/// <summary>
		/// Removes the post.
		/// </summary>
		/// <param name="postid">Postid.</param>
		public abstract void RemovePost (long postid);

		/// <summary>
		/// Removes the comment.
		/// </summary>
		/// <returns>The comment.</returns>
		/// <param name="cmtid">Cmtid.</param>
		public abstract long RemoveComment (long cmtid);

		/// <summary>
		/// Lasts the posts.
		/// </summary>
		/// <returns>The posts.</returns>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		/// <param name="totalRecords">Total records.</param>
		public abstract BlogEntryCollection LastPosts(int pageIndex, int pageSize, out int totalRecords);

		/// <summary>
		/// Blogs the title.
		/// </summary>
		/// <returns>The title.</returns>
		/// <param name="username">Username.</param>
		public abstract string BlogTitle (string username);

		/// <summary>
		/// Comment the specified from, postid and content.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="postid">Postid.</param>
		/// <param name="content">Content.</param>
		public abstract long Comment (string from, long postid, string content);

		/// <summary>
		/// Gets the comments.
		/// </summary>
		/// <returns>The comments.</returns>
		/// <param name="postid">Postid.</param>
		/// <param name="getHidden">If set to <c>true</c> get hidden.</param>
		public abstract Comment[] GetComments (long postid, bool getHidden) ;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Blogs.BlogProvider"/> auto validate comment.
		/// </summary>
		/// <value><c>true</c> if auto validate comment; otherwise, <c>false</c>.</value>
		public abstract bool AutoValidateComment { get; set; }

		/// <summary>
		/// Validates the comment.
		/// </summary>
		/// <param name="cmtid">Cmtid.</param>
		public abstract void ValidateComment (long cmtid);

		/// <summary>
		/// Updates the comment.
		/// </summary>
		/// <param name="cmtid">Cmtid.</param>
		/// <param name="content">Content.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		public abstract void UpdateComment (long cmtid, string content, bool visible);

		/// <summary>
		/// Tag the specified postid and tag.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="tag">Tag.</param>
		public abstract long Tag (long postid,string tag);

		/// <summary>
		/// Removes the tag.
		/// </summary>
		/// <param name="tagid">Tagid.</param>
		public abstract void RemoveTag (long tagid);
	}

}

