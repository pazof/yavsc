using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Generic;

namespace Yavsc.Model.Blogs
{
	public abstract class BlogProvider: ProviderBase
	{
		public abstract BlogEntry GetPost (long postid);
		public abstract BlogEntry GetPost (string username, string title);
		public abstract long GetPostId (string username, string title);

		public abstract long Post (string username, string title, string content, bool visible);
		public abstract void UpdatePost (long postid, string title, string content, bool visible);
		public abstract BlogEntryCollection FindPost (string pattern, FindBlogEntryFlags searchflags, 
			int pageIndex, int pageSize, out int totalRecords);
		public abstract void RemovePost (string username, string title);
		public abstract void RemovePost (long postid);
		public abstract long RemoveComment (long cmtid);
		public abstract BlogEntryCollection LastPosts(int pageIndex, int pageSize, out int totalRecords);
		public abstract string BlogTitle (string username);
		public abstract long Comment (string from, long postid, string content);
		public abstract Comment[] GetComments (long postid, bool getHidden) ;
		public abstract bool AutoValidateComment { get; set; }
		public abstract void ValidateComment (long cmtid);
		public abstract void UpdateComment (long cmtid, string content, bool visible);
		public abstract long Tag (long postid,string tag);
		public abstract void RemoveTag (long tagid);
	}

}

