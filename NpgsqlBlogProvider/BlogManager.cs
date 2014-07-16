using System;
using Npgsql.Web.Blog.DataModel;

namespace Npgsql.Web.Blog
{
	public static class BlogManager
	{
		public static long RemoveComment(long cmtid)
		{
			return Provider.RemoveComment (cmtid);
		}

		public static void Comment (string from, long postid, string content, bool visible)
		{
			provider.Comment (from, postid, content);
		}

		static BlogProvider provider;

		public static BlogProvider Provider {
			get {
				if (provider == null)
					provider = BlogHelper.GetProvider();
				return provider;
			}
		}
		public static BlogEntry GetPost (string username, string title)
		{
			return Provider.GetPost (username, title );
		}
		public static BlogEntry GetPost(long postid)
		{
			return Provider.GetPost (postid);
		}
		public static void Post(string username, string title, string content, bool visible)
		{
			Provider.Post(username, title, content, visible );
		}
		public static void UpdatePost(long postid, string content, bool visible)
		{
			Provider.UpdatePost(postid, content, visible);
		}
		public static BlogEntryCollection FindPost (string pattern, FindBlogEntryFlags searchflags, int pageIndex, int pageSize, out int totalRecords)
		{
			return Provider.FindPost (pattern, searchflags, pageIndex, pageSize, out totalRecords);
		}
		public static void RemovePost (string username, string title)
		{
			Provider.RemovePost (username, title);
		}
		public static BlogEntryCollection LastPosts (int pageIndex, int pageSize, out int totalRecords)
		{
			return Provider.LastPosts (pageIndex, pageSize, out totalRecords);
		}
		public static Comment[] GetComments(long postid, bool getHidden=true)
		{
			return Provider.GetComments (postid,getHidden);
		}
	}
}

