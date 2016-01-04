using System;
using Yavsc.Model.Blogs;
using Yavsc.Model.RolesAndMembers;
using System.Web;
using System.Web.Security;
using Yavsc.Model.Circles;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace Yavsc.Model.Blogs
{
	
	/// <summary>
	/// Blog manager.
	/// </summary>
	public static class BlogManager 
	{
		static BlogProvider provider = null;
		static BlogProvider Provider { 
			get {
				if (provider == null)
					provider = ManagerHelper.CreateDefaultProvider<BlogProvider>
						("system.web/blog"); 
				return provider;
			} 
		}
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
			Provider.Comment (from, postid, content);
		}

		/// <summary>
		/// Gets the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		public static UUTBlogEntryCollection GetPost (string username, string title)
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
		public static long Post (string username, string title, string content, bool visible, long[] cids)
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
		public static void UpdatePost (long postid, string title, string content, bool visible, long[] cids)
		{
			Provider.UpdatePost (postid, title, content, visible, cids);
		}

		/// <summary>
		/// Updates the post.
		/// </summary>
		/// <param name="be">Be.</param>
		public static void UpdatePost (BlogEntry be)
		{
			Provider.UpdatePost (be);
		}
		/// <summary>
		/// Updates the post photo.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="photo">Photo.</param>
		public static void UpdatePostPhoto (long postid, string photo)
		{
			Provider.UpdatePostPhoto (postid, photo);
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
		/// <param name="post_id">Post identifier.</param>
		public static void RemovePost (long post_id)
		{
			Provider.RemovePost (post_id);
		}

		/// <summary>
		/// Removes the post.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		public static void RemoveTitle (string username, string title)
		{
			if (!Roles.IsUserInRole ("Admin")) {
				string rguser = Membership.GetUser ().UserName;
				if (rguser != username) {
					throw new AccessViolationException (
						string.Format (
							"{1}, Vous n'avez pas le droit de suprimer les Blogs de {0}",
							username, rguser));
				}
			}
			Provider.RemoveTitle (username, title);
		}
		/// <summary>
		/// Gets the tag info.
		/// </summary>
		/// <returns>The tag info.</returns>
		/// <param name="tagname">Tagname.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		public static TagInfo GetTagInfo(string tagname, int pageIndex=0, int pageSize=50)
		{
			var res = new TagInfo (tagname);
			int recordCount = 0;
			var posts = Provider.FindPost (null,tagname,FindBlogEntryFlags.MatchTag,pageIndex,pageSize,out recordCount);
			res.Titles = posts.GroupByTitle ().ToArray();
			res.Name = tagname;
			// out int recordCount ,
			return res;
		}
		/// <summary>
		/// Lasts the posts.
		/// </summary>
		/// <returns>The posts.</returns>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		/// <param name="totalRecords">Total records.</param>
		public static IEnumerable<BlogEntry> LastPosts (int pageIndex, int pageSize, out int totalRecords)
		{
			var c =  Provider.LastPosts (pageIndex, pageSize, out totalRecords);
			return FilterOnReadAccess (c);
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
		public static void Tag (long postid, string tag)
		{
			Provider.Tag (postid, tag);
		}

		/// <summary>
		/// Rate the specified postid and rate.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="rate">Rate.</param>
		public static void Rate (long postid, int rate)
		{
			Provider.Rate (postid, rate);
		}

		/// <summary>
		/// Checks the auth can edit.
		/// </summary>
		/// <returns><c>true</c>, if auth can edit was checked, <c>false</c> otherwise.</returns>
		/// <param name="postid">Postid.</param>
		/// <param name="throwEx">If set to <c>true</c> throw ex.</param>
		public static BlogEntry GetForEditing (long postid, bool throwEx = true)
		{
			BlogEntry e = BlogManager.GetPost (postid);
			if (e == null)
				throw new PostNotFoundException ();
			if (!Roles.IsUserInRole ("Admin")) {
				string rguser = Membership.GetUser ().UserName;
				if (rguser != e.Author) {
					if (throwEx)
						throw new AccessViolationException (
							string.Format (
								"Vous n'avez pas le droit d'editer ce billet (id:{0})",
								e.Id));
					else
						return null;
					
				}
			}
			return e;
		}

		/// <summary>
		/// Untag the specified postid and tagname.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="tagname">Tagname.</param>
		public static void Untag (long postid, string tagname)
		{
			Provider.Untag (postid, tagname);
		}

		private static bool CanView (BlogEntry e, MembershipUser u = null)
		{
			if (e.AllowedCircles != null && e.AllowedCircles.Length > 0) {
				// only deliver to admins, owner, or specified circle memebers
				if (u == null)
					return false;
				if (u.UserName != e.Author)
				if (!Roles.IsUserInRole (u.UserName, "Admin"))
				{
					if (!e.Visible)
						return false;
					if (!CircleManager.DefaultProvider.Matches (e.AllowedCircles, u.UserName))
						return false;
				}
			}
			return true;
		}
		/// <summary>
		/// Checks the auth can read.
		/// </summary>
		/// <returns><c>true</c>, if auth can read was checked, <c>false</c> otherwise.</returns>
		/// <param name="postid">Postid.</param>
		/// <param name="throwEx">If set to <c>true</c> throw ex.</param>
		public static BlogEntry GetForReading (long postid, bool throwEx = true)
		{
			BlogEntry e = BlogManager.GetPost (postid);
			if (e == null) 
			if (throwEx)
				throw new FileNotFoundException ();
			if ( CanView (e, Membership.GetUser ()))
				return e;
			if (throwEx)
				throw new AccessViolationException (string.Format (
					"Vous n'avez pas le droit de lire ce billet (id:{0})",
					postid.ToString ()));
			return null;
			
		}

		/// <summary>
		/// Checks the auth can read.
		/// </summary>
		/// <returns><c>true</c>, if auth can read was checked, <c>false</c> otherwise.</returns>
		/// <param name="bec">Bec.</param>
		/// <param name="throwEx">If set to <c>true</c> throw ex.</param>
		private static bool HasReadAccess (BlogEntryCollection bec, bool throwEx = true)
		{
			if (bec == null)
				throw new FileNotFoundException ();
			if (Roles.IsUserInRole ("Admin"))
				return true;
			var u = Membership.GetUser ();
			BlogEntry e = bec.First (x=>!CanView(x,u));
			if (e == null)
				return true;
			if (throwEx)
				throw new AccessViolationException (
					string.Format (
						"Vous n'avez pas le droit de lire cette collection de billet (titles:{0})",
						bec.ToString()));
			else
				return false;
		}
		/// <summary>
		/// Filters the on read access.
		/// </summary>
		/// <returns>The on read access.</returns>
		/// <param name="bec">Bec.</param>
		/// <typeparam name="TEntry">The 1st type parameter.</typeparam>
		public static IEnumerable<TEntry> FilterOnReadAccess<TEntry> ( IEnumerable<TEntry> bec)
		{
			if (bec == null) return null;
			if (Roles.IsUserInRole ("Admin")) return bec;
			var u = Membership.GetUser ();
			var r = bec.Where (x => CanView (x as BlogEntry, u));
			return r;
		}
		/// <summary>
		/// Gets the post counter.
		/// </summary>
		/// <returns>The count.</returns>
		/// <param name="bloggerName">Blogger name.</param>
		public static long GetPostCounter(string bloggerName)
		{
			return Provider.GetPublicPostCount (bloggerName);
		}
	}
}

