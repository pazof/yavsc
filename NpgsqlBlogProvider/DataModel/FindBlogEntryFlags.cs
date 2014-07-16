using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Generic;

namespace Npgsql.Web.Blog.DataModel
{

	public enum FindBlogEntryFlags : byte {
		MatchTitle = 1,
		MatchContent = 2,
		MatchUserName = 4,
		MatchInvisible = 8
	}
	
}
