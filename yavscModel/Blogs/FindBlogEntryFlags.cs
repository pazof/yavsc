using System;
using System.Configuration;
using System.Collections.Generic;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Find blog entry flags.
	/// </summary>
	public enum FindBlogEntryFlags : byte {
		/// <summary>
		/// The match title.
		/// </summary>
		MatchTitle = 1,
		/// <summary>
		/// The content of the match.
		/// </summary>
		MatchContent = 2,
		/// <summary>
		/// The name of the match user.
		/// </summary>
		MatchUserName = 4,
		/// <summary>
		/// The match invisible.
		/// </summary>
		MatchInvisible = 8
	}
	
}
