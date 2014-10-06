using System;
using System.Configuration;
using System.Collections.Generic;

namespace Yavsc.Model.Blogs
{

	public enum FindBlogEntryFlags : byte {
		MatchTitle = 1,
		MatchContent = 2,
		MatchUserName = 4,
		MatchInvisible = 8
	}
	
}
