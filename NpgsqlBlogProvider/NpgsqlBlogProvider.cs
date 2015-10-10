using System;
using System.Configuration;
using System.Configuration.Provider;
using Npgsql;
using System.Collections.Generic;
using Yavsc.Model.Blogs;
using Yavsc.Model.Circles;
using System.Web.Mvc;
using NpgsqlTypes;

namespace Npgsql.Web.Blog
{
	/// <summary>
	/// Npgsql blog provider.
	/// </summary>
	public class NpgsqlBlogProvider : BlogProvider
	{
		string applicationName;
		string connectionString;

		#region implemented abstract members of BlogProvider
		/// <summary>
		/// Tag the specified postid and tag.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="tag">Tag.</param>
		public override long Tag (long postid, string tag)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "insert into bltag (blid,tag) values (@postid,@tag) returning _id";
				cmd.Parameters.AddWithValue("@tag",tag);
				cmd.Parameters.AddWithValue("@postid",postid);
				cnx.Open ();
				return (long) cmd.ExecuteScalar ();
			}
		}
		/// <summary>
		/// Removes the tag.
		/// </summary>
		/// <param name="tagid">Tagid.</param>
		public override void RemoveTag (long tagid)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "delete from bltag where _id = @tagid";
				cmd.Parameters.AddWithValue("@tagid",tagid);
				cnx.Open ();
				cmd.ExecuteNonQuery ();	
			}
		}
		/// <summary>
		/// Gets the comments.
		/// </summary>
		/// <returns>The comments.</returns>
		/// <param name="postid">Postid.</param>
		/// <param name="getHidden">If set to <c>true</c> get hidden.</param>
		public override Comment[] GetComments (long postid, bool getHidden)
		{
			List<Comment> cmts = new List<Comment> ();

			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {

				cmd.CommandText = "select _id, username, bcontent, modified, posted, visible from comment " +
				                  "where applicationname = @appname and postid = @id" + 
				                  ((getHidden) ?  " and visible = true ":" ") + 
				                  "order by posted asc" ;
				cmd.Parameters.AddWithValue ("@appname", applicationName);
				cmd.Parameters.AddWithValue ("@id", postid);
				cnx.Open ();
				using (NpgsqlDataReader rdr = cmd.ExecuteReader()) {
					while (rdr.Read ()) {
						Comment c = new Comment();
						c.CommentText = rdr.GetString (rdr.GetOrdinal ("bcontent"));
						c.From = rdr.GetString (rdr.GetOrdinal ("username"));
						c.Modified = rdr.GetDateTime (rdr.GetOrdinal ("modified"));
						c.Posted = rdr.GetDateTime (rdr.GetOrdinal ("posted"));
						c.Visible = rdr.GetBoolean (rdr.GetOrdinal ("visible"));
						c.PostId = postid;
						c.Id = rdr.GetInt64(rdr.GetOrdinal("_id"));
						cmts.Add (c);
					}
				}
			}
			return cmts.ToArray();
		}
		/// <summary>
		/// Updates the post.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="title">Title.</param>
		/// <param name="content">Content.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		/// <param name="cids">Circle identifiers</param>
		public override void UpdatePost (long postid, string title, string content, 
			bool visible, long [] cids)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection(connectionString)) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					DateTime now = DateTime.Now;
					cmd.CommandText = 
					"update blog set modified=@now," +
					" title = @title," +
					" bcontent=@content, " +
					" visible = @visible " +
					"where _id = @id";
					cmd.Parameters.AddWithValue ("@now", now);
					cmd.Parameters.AddWithValue ("@title", title);
					cmd.Parameters.AddWithValue ("@content", content);
					cmd.Parameters.AddWithValue ("@visible", visible);
					cmd.Parameters.AddWithValue ("@id", postid);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
				}
				cnx.Close();
			}
			UpdatePostCircles (postid, cids);
		}
		/// <summary>
		/// Removes the post.
		/// </summary>
		/// <param name="postid">Postid.</param>
		public override void RemovePost (long postid)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "delete from blog where _id = @id";
				cmd.Parameters.AddWithValue ("id", postid);
				cnx.Open ();
				cmd.ExecuteNonQuery();
			}
		}
		/// <summary>
		/// Comment the specified from, postid and content.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="postid">Postid.</param>
		/// <param name="content">Content.</param>
		public override long Comment (string from, long postid, string content)
		{
			if (from == null)
				throw new ArgumentNullException("from");
			if (content == null)
				throw new ArgumentNullException("content");
			bool visible = AutoValidatesComments;
			using (NpgsqlConnection cnx=
				new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {
				cmd.CommandText = "insert into comment (postid,bcontent," +
					"modified,posted,visible,username,applicationname)" +
				    "values (@postid,@bcontent,@modified,@posted," +
				    "@visible,@username,@appname) returning _id";
				cmd.Parameters.AddWithValue ("@postid", postid);
				cmd.Parameters.AddWithValue ("@bcontent", content);
				DateTime now = DateTime.Now;
				cmd.Parameters.AddWithValue ("@modified", now);
				cmd.Parameters.AddWithValue ("@posted", now);
				cmd.Parameters.AddWithValue ("@visible", visible);
				cmd.Parameters.AddWithValue ("@username", from);
				cmd.Parameters.AddWithValue ("@appname", applicationName);
				cnx.Open ();
				return (long) cmd.ExecuteScalar();
			}
		}
		/// <summary>
		/// Validates the comment.
		/// </summary>
		/// <param name="cmtid">Cmtid.</param>
		public override void ValidateComment (long cmtid)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Updates the comment.
		/// </summary>
		/// <param name="cmtid">Cmtid.</param>
		/// <param name="content">Content.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		public override void UpdateComment 
		(long cmtid, string content, bool visible)
		{
			throw new NotImplementedException ();
		}

		private bool autoValidateComment = true;
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Npgsql.Web.Blog.NpgsqlBlogProvider"/> auto validate comment.
		/// </summary>
		/// <value><c>true</c> if auto validate comment; otherwise, <c>false</c>.</value>
		public override bool AutoValidatesComments {
			get {
				return autoValidateComment;
			}
			set {
				autoValidateComment=value;
			}
		}

		/// <summary>
		/// Blogs the title.
		/// </summary>
		/// <returns>The title.</returns>
		/// <param name="username">Username.</param>
		public override string BlogTitle
		 (string username)
		{
			throw new NotImplementedException ();
		}

		#endregion
		/// <summary>
		/// Initialize the specified name and config.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="config">Config.</param>
		public override void Initialize 
		(string name, System.Collections.Specialized.NameValueCollection config)
		{
			string cnxName = config ["connectionStringName"];
			connectionString = ConfigurationManager.ConnectionStrings [cnxName].ConnectionString;
			config.Remove ("connectionStringName");
			applicationName = config ["applicationName"];
			config.Remove ("applicationName");
			defaultPageSize = int.Parse ( config ["pageLen"] ?? "10") ;
			base.Initialize (name, config);
		}
		#region implemented abstract members of BlogProvider
		/// <summary>
		/// Gets the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="postid">Postid.</param>
		public override BlogEntry GetPost (long postid)
		{
			BlogEntry be = null;
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {
				cmd.CommandText = "select username, title, bcontent, modified, posted, visible, photo from blog " +
				                  "where applicationname = @appname and _id = @id";
				cmd.Parameters.AddWithValue ("@appname", applicationName);
				cmd.Parameters.AddWithValue ("@id", postid);
				cnx.Open ();
				using (NpgsqlDataReader rdr = cmd.ExecuteReader()) {
					if (rdr.Read ()) {
						be = new BlogEntry ();
						be.Title = rdr.GetString (rdr.GetOrdinal ("title"));
						be.Content = rdr.GetString (rdr.GetOrdinal ("bcontent"));
						be.Author = rdr.GetString (rdr.GetOrdinal ("username"));
						be.Modified = rdr.GetDateTime (rdr.GetOrdinal ("modified"));
						be.Posted = rdr.GetDateTime (rdr.GetOrdinal ("posted"));
						be.Visible = rdr.GetBoolean (rdr.GetOrdinal ("visible"));
						int oph = rdr.GetOrdinal ("photo");
						if (!rdr.IsDBNull(oph))
							be.Photo = rdr.GetString (oph);
						be.Id = postid;
					}
				}
			}
			if (be!=null) SetCirclesOn (be);
			return be;

		}
		/// <summary>
		/// Removes the comment.
		/// </summary>
		/// <returns>The comment.</returns>
		/// <param name="cmtid">Cmtid.</param>
		public override long RemoveComment (long cmtid)
		{
			long postid = 0;
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "delete from comment where _id = @id returning postid";
				cmd.Parameters.AddWithValue ("id", cmtid);
				cnx.Open ();
				postid = (long) cmd.ExecuteScalar ();
			}
			return postid;
		}
		/// <summary>
		/// Gets the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		public override UUTBlogEntryCollection GetPost (string username, string title)
		{
			UUTBlogEntryCollection bec = new UUTBlogEntryCollection (username,title);
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "select _id,bcontent,modified,posted,visible,photo from blog " +
					"where applicationname = :appname and username = :username and title = :title";
					cmd.Parameters.AddWithValue ("appname", NpgsqlDbType.Varchar, applicationName);
					cmd.Parameters.AddWithValue ("username", NpgsqlDbType.Varchar ,username);
					cmd.Parameters.AddWithValue ("title", NpgsqlDbType.Varchar, title);
					cnx.Open ();
					cmd.Prepare ();
					using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
						while (rdr.Read ()) {
							BlogEntry be = new BlogEntry ();
							be.Title = title;
							be.Content = rdr.GetString (rdr.GetOrdinal ("bcontent"));
							be.Author = username;
							be.Modified = rdr.GetDateTime (rdr.GetOrdinal ("modified"));
							be.Posted = rdr.GetDateTime (rdr.GetOrdinal ("posted"));
							be.Visible = rdr.GetBoolean (rdr.GetOrdinal ("visible"));
							be.Id = rdr.GetInt64 (rdr.GetOrdinal ("_id"));
							{
								int oph = rdr.GetOrdinal ("photo");
								if (!rdr.IsDBNull (oph))
									be.Photo = rdr.GetString (oph);
							}
							bec.Add (be);
						}
						rdr.Close ();
					}

				}
				if (bec.Count != 0) {
					using (NpgsqlCommand cmdtags = cnx.CreateCommand ()) {
						long pid = 0;
						cmdtags.CommandText = "select tag.name from tag,tagged where tag._id = tagged.tagid and tagged.postid = :postid";
						cmdtags.Parameters.AddWithValue ("postid", NpgsqlTypes.NpgsqlDbType.Bigint, pid);
						cmdtags.Prepare ();
						foreach (BlogEntry be in bec) {
							List<string> tags = new List<string> ();
							cmdtags.Parameters ["postid"].Value = be.Id;
							using (NpgsqlDataReader rdrt = cmdtags.ExecuteReader ()) {
								while (rdrt.Read ()) {
									tags.Add (rdrt.GetString (0));
								}
							}
							be.Tags = tags.ToArray ();
						}
					}

					SetCirclesOn (bec);
				}
			}
			return bec;
		}

		private void SetCirclesOn(BlogEntry be)
		{
			List<long> circles = new List<long> ();
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmdcircles = cnx.CreateCommand ()) {
				cmdcircles.CommandText = "select a.circle_id from blog_access a " +
					"where a.post_id = :pid";
				cmdcircles.Parameters.AddWithValue ("pid", be.Id);
				cnx.Open ();
				using (NpgsqlDataReader rdr = cmdcircles.ExecuteReader ()) {
					while (rdr.Read ()) {
						circles.Add (rdr.GetInt64 (0));
					}
				}
			}
			be.AllowedCircles = circles.ToArray ();
		}

		private void SetCirclesOn(BlogEntryCollection bec)
			{
			foreach (BlogEntry be in bec) {
				SetCirclesOn (be);
			}
		}
		/// <summary>
		/// Post the specified username, title, content and visible.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		/// <param name="content">Content.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		/// <param name="circles">.</param>
		public override long Post (string username, string title, string content, bool visible, long [] circles)
		{
			long pid = 0;
			if (username == null)
				throw new ArgumentNullException("username");
			if (title == null)
				throw new ArgumentNullException("title");
			if (content == null)
				throw new ArgumentNullException("content");
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "insert into blog (title,bcontent,modified,posted,visible,username,applicationname)" +
					"values (:title,:bcontent,:modified,:posted,:visible,:username,:appname) returning _id";
					cmd.Parameters.AddWithValue ("title", title);
					cmd.Parameters.AddWithValue ("bcontent", content);
					DateTime now = DateTime.Now;
					cmd.Parameters.AddWithValue ("modified", now);
					cmd.Parameters.AddWithValue ("posted", now);
					cmd.Parameters.AddWithValue ("visible", visible);
					cmd.Parameters.AddWithValue ("username", username);
					cmd.Parameters.AddWithValue ("appname", applicationName);
					cnx.Open ();
					pid = (long)cmd.ExecuteScalar ();
				}
				cnx.Close ();
			}
			UpdatePostCircles (pid, circles);
			return pid;
		}
		/// <summary>
		/// Updates the post photo.
		/// </summary>
		/// <param name="pid">Pid.</param>
		/// <param name="photo">Photo.</param>
		public override void UpdatePostPhoto ( long pid, string photo)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "update blog set photo = :photo where _id = :pid";
					cmd.Parameters.AddWithValue ("pid", pid);
					cmd.Parameters.AddWithValue ("photo", photo);
					cmd.ExecuteNonQuery ();
				}
				cnx.Close ();
			}
		}

		private void UpdatePostCircles( long pid, long[] circles)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "delete from blog_access where post_id = :pid";
					cmd.Parameters.AddWithValue ("pid", pid);
					cmd.ExecuteNonQuery ();
				}
				if (circles!=null)
				if (circles.Length>0)
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "insert into blog_access (post_id,circle_id) values (:pid,:cid)";
					cmd.Parameters.AddWithValue ("pid", NpgsqlTypes.NpgsqlDbType.Bigint, pid);
					cmd.Parameters.Add ("cid", NpgsqlTypes.NpgsqlDbType.Bigint);
					cmd.Prepare ();
					foreach (long ci in circles) {
						cmd.Parameters ["cid"].Value = ci;
						cmd.ExecuteNonQuery ();
					}
				}
				cnx.Close ();
			}
		}
		/// <summary>
		/// Finds the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="readersName">Reader's Name.</param>
		/// <param name="pattern">Pattern.</param>
		/// <param name="searchflags">Searchflags.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		/// <param name="totalRecords">Total records.</param>
		public override BlogEntryCollection FindPost (string readersName, string pattern, FindBlogEntryFlags searchflags, int pageIndex, int pageSize, out int totalRecords)
		{
			BlogEntryCollection c = new BlogEntryCollection ();
			totalRecords = 0;
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {
				if (readersName != null) {
					cmd.CommandText = "select _id, title,bcontent, modified," +
						"posted,username,visible " +
						"from blog b left outer join " +
						"(select count(*)>0 acc, a.post_id pid " +
						"from blog_access a," +
						" circle_members m, users u where m.circle_id = a.circle_id " +
						" and m.member = u.username and u.username = :uname " +
						" and u.applicationname = :appname " +
						" group by a.post_id) ma on (ma.pid = b._id) " +
						"where ( ((ma.acc IS NULL or ma.acc = TRUE) and b.Visible IS TRUE ) or b.username = :uname) ";
					cmd.Parameters.AddWithValue ("uname", readersName);
				} else {
					cmd.CommandText = "select _id, title,bcontent,modified," +
						"posted,username,visible " +
						"from blog b left outer join " +
						"(select count(*)>0 acc, a.post_id pid " +
						"from blog_access a" +
						" group by a.post_id) ma on (ma.pid = b._id)" +
						" where " +
						" ma.acc IS NULL and  " +
						" b.Visible IS TRUE and  " +
						" applicationname = :appname";
				}
				cmd.Parameters.AddWithValue ("@appname", applicationName);
				if ((searchflags & FindBlogEntryFlags.MatchContent) > 0) {
					cmd.CommandText += " and bcontent like :bcontent";
					cmd.Parameters.AddWithValue (":bcontent", pattern);
				}
				if ((searchflags & FindBlogEntryFlags.MatchTitle) > 0) {
					cmd.CommandText += " and title like :title";
					cmd.Parameters.AddWithValue (":title", pattern);
				}
				if ((searchflags & FindBlogEntryFlags.MatchUserName) > 0) {
					cmd.CommandText += " and username like :username";
					cmd.Parameters.AddWithValue (":username", pattern);
				}
				if ((searchflags & FindBlogEntryFlags.MatchInvisible) == 0) {
					cmd.CommandText += " and visible = true";
				}

				cmd.CommandText += " order by posted desc";
				cnx.Open ();
				using (NpgsqlDataReader rdr = cmd.ExecuteReader()) {
					// pageIndex became one based
					int firstrec = pageIndex * pageSize;
					int lastrec = firstrec + pageSize - 1;
					while (rdr.Read()) {
						if (totalRecords >= firstrec && totalRecords <= lastrec) {
							BlogEntry be = new BlogEntry ();
							be.Title = rdr.GetString (rdr.GetOrdinal ("title"));
							be.Id = rdr.GetInt64 (rdr.GetOrdinal ("_id"));
							be.Content = rdr.GetString (rdr.GetOrdinal ("bcontent"));
							be.Author = rdr.GetString (rdr.GetOrdinal ("username"));
							be.Posted = rdr.GetDateTime (rdr.GetOrdinal ("posted"));
							be.Modified = rdr.GetDateTime (rdr.GetOrdinal ("modified"));
							be.Visible =  rdr.GetBoolean (rdr.GetOrdinal ("visible"));
							c.Add (be);
						}
						totalRecords++;
					}
					rdr.Close ();
				}
			}
			foreach (BlogEntry be in c)
				SetCirclesOn (be);
			
			return c;
		}
		/// <summary>
		/// Removes the post.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		public override void RemoveTitle (string username, string title)
		{
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {
				cmd.CommandText = "delete from blog where username = @username and applicationname = @appname and title=@title";
				cmd.Parameters.AddWithValue ("@username",username);
				cmd.Parameters.AddWithValue ("@appname", applicationName);
				cmd.Parameters.AddWithValue ("@title",title);
				cnx.Open ();
				cmd.ExecuteNonQuery ();
				cnx.Close();
			}
		}


		int defaultPageSize = 10;
		/// <summary>
		/// Lasts the posts.
		/// </summary>
		/// <returns>The posts.</returns>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		/// <param name="totalRecords">Total records.</param>
		public override BlogEntryCollection LastPosts(int pageIndex, int pageSize, out int totalRecords)
		{
			BlogEntryCollection c = new BlogEntryCollection ();
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {

				/*cmd.CommandText = "select blog.* from blog, " +
					"(select max(posted) lpost, username " +
					"from blog where applicationname = @appname " +
					"group by username) as lblog " +
					"where blog.posted = lblog.lpost and blog.username = lblog.username " ;
					*/
				cmd.CommandText = "select * " +
				                  "from blog where applicationname = :appname and visible = true " +
				                  " order by posted desc limit :len" ;

				cmd.Parameters.AddWithValue ("appname", applicationName);
				cmd.Parameters.AddWithValue ("len", defaultPageSize*10);
				cnx.Open ();
				using (NpgsqlDataReader rdr = cmd.ExecuteReader()) {
					totalRecords = 0;
					int firstrec = pageIndex * pageSize;
					int lastrec = firstrec + pageSize - 1;
					while (rdr.Read()) {
						if (totalRecords >= firstrec && totalRecords <= lastrec) {
							BlogEntry be = new BlogEntry ();
							be.Id = rdr.GetInt64 (rdr.GetOrdinal ("_id"));
							be.Title = rdr.GetString (rdr.GetOrdinal ("title"));
							be.Content = rdr.GetString (rdr.GetOrdinal ("bcontent"));
							be.Author = rdr.GetString (rdr.GetOrdinal ("username"));
							be.Posted = rdr.GetDateTime (rdr.GetOrdinal ("posted"));
							be.Modified = rdr.GetDateTime (rdr.GetOrdinal ("modified"));
							be.Visible = true; // because of sql code used
							c.Add (be);
						}
						totalRecords++;
					}
				}
			}
			foreach (BlogEntry be in c)
				SetCirclesOn (be);
			return c;
		}
		#endregion
	}
}
