using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Generic;
using Yavsc.Model.Circles;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Blog provider.
	/// </summary>
	public abstract class BlogProvider: ProviderBase
	{
		/// <summary>
		/// Gets the public post count.
		/// </summary>
		/// <returns>The public post count.</returns>
		/// <param name="bloggerName">Blogger name.</param>
		public abstract long GetPublicPostCount (string bloggerName);

		/// <summary>
		/// Gets the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="postid">Postid.</param>
		public abstract BlogEntry GetPost (long postid);


		/// <summary>
		/// Gets the post collection from a given user and at a given title.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		public abstract UUTBlogEntryCollection GetPost (string username, string title);

		/// <summary>
		/// Saves a post from the given username, 
		/// at the specified title, this content, 
		/// visible or not, and for allowedCircles.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		/// <param name="content">Content.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		/// <param name="allowedCircles">Allowed circles.</param>
		public abstract long Post (string username, string title, string content, bool visible, long[] allowedCircles);

		/// <summary>
		/// Note the specified postid and note.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="rate">rate.</param>
		public abstract void Rate (long postid, int rate);


		/// <summary>
		/// Updates the post specified by its id,
		/// using the given title, content, visibility and circle collection.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="title">Title.</param>
		/// <param name="content">Content.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		/// <param name="allowedCircles">Allowed circles.</param>
		public abstract void UpdatePost (long postid, string title, string content, bool visible, long[] allowedCircles);

		/// <summary>
		/// Updates the post.
		/// </summary>
		/// <param name="be">Be.</param>
		public abstract void UpdatePost ( BlogEntry be );


		/// <summary>
		/// Finds a post.
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
		/// Removes the posts under the specified title and user.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		public abstract void RemoveTitle (string username, string title);

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
		/// Lasts the posts for this application.
		/// </summary>
		/// <returns>The posts.</returns>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		/// <param name="totalRecords">Total records.</param>
		public abstract BlogEntryCollection LastPosts(int pageIndex, int pageSize, out int totalRecords);

		/// <summary>
		/// Returns the user's blog title.
		/// </summary>
		/// <returns>The title.</returns>
		/// <param name="username">Username.</param>
		public abstract string BlogTitle (string username);

		/// <summary>
		/// Saves a comment from specified user
		/// on the specided post using the specified content.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="postid">Postid.</param>
		/// <param name="content">Content.</param>
		public abstract long Comment (string from, long postid, string content);

		/// <summary>
		/// Gets the comments on a specided post by identifier <c>postid</c>.
		/// </summary>
		/// <returns>The comments.</returns>
		/// <param name="postid">Postid.</param>
		/// <param name="getHidden">If set to <c>true</c> get hidden.</param>
		public abstract Comment[] GetComments (long postid, bool getHidden) ;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Blogs.BlogProvider"/> auto validates comments.
		/// </summary>
		/// <value><c>true</c> if auto validate comment; otherwise, <c>false</c>.</value>
		public abstract bool AutoValidatesComments { get; set; }

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
		/// Tag the specified post by identifier 
		/// using the given tag.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="tag">Tag.</param>
		public abstract long Tag (long postid, string tag);

		/// <summary>
		/// Uns the tag.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="tagid">Tagid.</param>
		public abstract void Untag (long postid, long tagid);

		/// <summary>
		/// Uns the tag.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="name">Name.</param>
		public abstract void Untag (long postid, string name);
		/// <summary>
		/// Removes the tag.
		/// </summary>
		/// <param name="tagid">Tagid.</param>
		public abstract void DropTag (long tagid);
		/// <summary>
		/// Updates the post photo.
		/// </summary>
		/// <param name="pid">Pid.</param>
		/// <param name="photo">Photo.</param>
		public abstract void UpdatePostPhoto (long pid, string photo);
	}

}

