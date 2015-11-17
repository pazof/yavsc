using System;
using System.Configuration;
using System.Collections.Generic;
using Yavsc.Model.Blogs;
using System.Linq;
using Yavsc.Model.Circles;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Blog entry collection.
	/// </summary>
	public class BlogEntryCollection : List<BlogEntry>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Blogs.BlogEntryCollection"/> class.
		/// </summary>
		public BlogEntryCollection ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Blogs.BlogEntryCollection"/> class.
		/// </summary>
		/// <param name="items">Items.</param>
		public BlogEntryCollection (IEnumerable<BlogEntry> items)
		{
			if (items != null)
				this.AddRange (items);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.Blogs.BlogEntryCollection"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.Blogs.BlogEntryCollection"/>.</returns>
		public override string ToString ()
		{
			string titles = Titles == null ?
				"none" : string.Join (", ", Titles);
			return string.Format ("[BlogEntryCollection: Titles={0}]", titles);
		}

		/// <summary>
		/// Get the specified bid.
		/// </summary>
		/// <param name="bid">Bid.</param>
		public BlogEntry Get (long bid)
		{
			return this.First (x => x.Id == bid);
		}

		/// <summary>
		/// Filters this collection on 
		/// the given title.
		/// </summary>
		/// <returns>The by title.</returns>
		/// <param name="title">Title.</param>
		public BlogEntry [] FilterOnTitle (string title)
		{
			return this.Where (x => x.Title == title).ToArray ();
		}

		/// <summary>
		/// Filters the current collection for a given user by its name.
		/// Assumes that this user is not an author of any of these posts.
		/// </summary>
		/// <param name="username">Username.</param>
		public BlogEntryCollection FilterFor (string username)
		{
			BlogEntryCollection res = new BlogEntryCollection ();
			foreach (BlogEntry be in this) {
				if (be.Visible &&
				    (be.AllowedCircles == null ||
				    (username != null && CircleManager.DefaultProvider.Matches
							(be.AllowedCircles, username)))) {
					res.Add (be);
				}
			}
			return res;
		}

		/// <summary>
		/// Groups by title.
		/// </summary>
		public IEnumerable<IGrouping<string,BasePostInfo>> GroupByTitle ()
		{
			return from be in this
				orderby be.Posted descending
				group new BasePostInfo(be)	by be.Title
				into titlegroup select titlegroup;
		}

		/// <summary>
		/// Groups by user.
		/// </summary>
		/// <returns>The by user.</returns>
		public IEnumerable<IGrouping<string,BasePostInfo>> GroupByUser ()
		{
			return from be in this
				orderby be.Posted descending
			       group
				new  BasePostInfo (be)
				by be.Author
				into usergroup
			       select usergroup;
		}

		/// <summary>
		/// Gets the titles.
		/// </summary>
		/// <value>The titles.</value>
		public string[] Titles {
			get { 
				string[] result = this.Select (x => x.Title).Distinct ().ToArray ();
				if (result == null)
					return new string[0];
				return result;
			}
		}
	}
}
