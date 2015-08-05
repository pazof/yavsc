using System;
using System.Configuration;
using System.Collections.Generic;
using Yavsc.Model.Blogs;
using System.Linq;

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
		public BlogEntryCollection(IEnumerable<BlogEntry> items)
		{
			if (items!=null)
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
		/// Post info.
		/// </summary>
		public struct PostInfoByTitle {

			/// <summary>
			/// The name of the user.
			/// </summary>
			public string UserName;
			/// <summary>
			/// The identifier.
			/// </summary>
			public long Id;
			/// <summary>
			/// The posted.
			/// </summary>
			public DateTime Posted;
			/// <summary>
			/// The modified.
			/// </summary>
			public DateTime Modified;

		}
		/// <summary>
		/// Post info by user.
		/// </summary>
		public struct PostInfoByUser {

			/// <summary>
			/// The name of the user.
			/// </summary>
			public string Title;
			/// <summary>
			/// The identifier.
			/// </summary>
			public long Id;
			/// <summary>
			/// The posted.
			/// </summary>
			public DateTime Posted;
			/// <summary>
			/// The modified.
			/// </summary>
			public DateTime Modified;

		}

		/// <summary>
		/// Groups by title.
		/// </summary>
		public IEnumerable<IGrouping<string,PostInfoByTitle>> GroupByTitle()
		{
			return from be in this
				orderby be.Posted descending
			        group
				new  PostInfoByTitle { UserName=be.UserName, Id=be.Id, 
				Posted=be.Posted, Modified=be.Modified }
				by be.Title
				into titlegroup
			        select titlegroup;
		}
		/// <summary>
		/// Groups by user.
		/// </summary>
		/// <returns>The by user.</returns>
		public IEnumerable<IGrouping<string,PostInfoByUser>> GroupByUser()
		{
			return from be in this
				orderby be.Posted descending
				group
				new  PostInfoByUser { Title=be.Title, Id=be.Id, 
				Posted=be.Posted, Modified=be.Modified }
				by be.UserName
				into usergroup
				select usergroup;
		}

		/// <summary>
		/// Gets the titles.
		/// </summary>
		/// <value>The titles.</value>
		public string[] Titles { get { 
				string[] result = this.Select (x => x.Title).Distinct ().ToArray ();
				if (result == null)
					return new string[0];
				return result;
			} }

		/// <summary>
		/// Gets a value indicating whether this <see cref="Yavsc.Model.Blogs.BlogEntryCollection"/> concerns A unique title.
		/// </summary>
		/// <value><c>true</c> if concerns A unique title; otherwise, <c>false</c>.</value>
		public bool ConcernsAUniqueTitle {
			get {
				if (this.Count <= 1)
					return true;
				else
					return this.All (x => Titles [0] == x.Title);
			}
		}
		/// <summary>
		/// Gets a value indicating whether this <see cref="Yavsc.Model.Blogs.BlogEntryCollection"/> concerns A unique title.
		/// </summary>
		/// <value><c>true</c> if concerns A unique title; otherwise, <c>false</c>.</value>
		public bool ConcernsAUniqueUser {
			get {
				if (this.Count <= 1)
					return true;
				else
					return this.All (x => x.UserName == this[0].UserName);
			}
		}

	}
	
}
