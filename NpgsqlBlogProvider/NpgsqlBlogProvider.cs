using System;
using System.Configuration;
using System.Configuration.Provider;
using Npgsql;
using System.Collections.Generic;
using yavscModel.Blogs;

namespace Npgsql.Web.Blog
{
	public class NpgsqlBlogProvider : BlogProvider
	{
		string applicationName;
		string connectionString;

		#region implemented abstract members of BlogProvider

		public override long GetPostId (string username, string title)
		{
			throw new NotImplementedException ();
		}
		public override Comment[] GetComments (long postid, bool getHidden)
		{
			List<Comment> cmts = new List<Comment> ();

			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {

				cmd.CommandText = "select _id, username, bcontent, modified, posted, visible from comment " +
				                  "where applicationname = @appname and postid = @id" + 
				                  ((getHidden) ?  " and visible = true ":" ") + 
				                  "order by posted asc" ;
				cmd.Parameters.Add ("@appname", applicationName);
				cmd.Parameters.Add ("@id", postid);
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
		public override void UpdatePost (long postid, string content, bool visible)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {
				DateTime now = DateTime.Now;
				cmd.CommandText = 
					"update blog set modified=@now, bcontent=@content, " +
					"visible = @visible where _id = @id";
				cmd.Parameters.Add ("@now", now);
				cmd.Parameters.Add ("@content", content);
				cmd.Parameters.Add ("@visible", visible);
				cmd.Parameters.Add ("@id", postid);
				cnx.Open ();
				cmd.ExecuteNonQuery ();
				cnx.Close();
			}
		}

		public override void RemovePost (long postid)
		{
			throw new NotImplementedException ();
		}

		public override long Comment (string from, long postid, string content)
		{
			if (from == null)
				throw new ArgumentNullException("from");
			if (content == null)
				throw new ArgumentNullException("content");
			bool visible = AutoValidateComment;
			using (NpgsqlConnection cnx=
				new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {
				cmd.CommandText = "insert into comment (postid,bcontent," +
					"modified,posted,visible,username,applicationname)" +
				    "values (@postid,@bcontent,@modified,@posted," +
				    "@visible,@username,@appname) returning _id";
				cmd.Parameters.Add ("@postid", postid);
				cmd.Parameters.Add ("@bcontent", content);
				DateTime now = DateTime.Now;
				cmd.Parameters.Add ("@modified", now);
				cmd.Parameters.Add ("@posted", now);
				cmd.Parameters.Add ("@visible", visible);
				cmd.Parameters.Add ("@username", from);
				cmd.Parameters.Add ("@appname", applicationName);
				cnx.Open ();
				return (long) cmd.ExecuteScalar();
			}
		}

		public override void ValidateComment (long cmtid)
		{
			throw new NotImplementedException ();
		}

		public override void UpdateComment 
		(long cmtid, string content, bool visible)
		{
			throw new NotImplementedException ();
		}

		private bool autoValidateComment = true;

		public override bool AutoValidateComment {
			get {
				return autoValidateComment;
			}
			set {
				autoValidateComment=value;
			}
		}


		public override string BlogTitle
		 (string username)
		{
			throw new NotImplementedException ();
		}

		#endregion

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
		public override BlogEntry GetPost (long postid)
		{
			BlogEntry be = null;
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {
				cmd.CommandText = "select username, title, bcontent, modified, posted, visible from blog " +
				                  "where applicationname = @appname and _id = @id";
				cmd.Parameters.Add ("@appname", applicationName);
				cmd.Parameters.Add ("@id", postid);
				cnx.Open ();
				using (NpgsqlDataReader rdr = cmd.ExecuteReader()) {
					if (rdr.Read ()) {
						be = new BlogEntry ();
						be.Title = rdr.GetString (rdr.GetOrdinal ("title"));
						be.Content = rdr.GetString (rdr.GetOrdinal ("bcontent"));
						be.UserName = rdr.GetString (rdr.GetOrdinal ("username"));
						be.Modified = rdr.GetDateTime (rdr.GetOrdinal ("modified"));
						be.Posted = rdr.GetDateTime (rdr.GetOrdinal ("posted"));
						be.Visible = rdr.GetBoolean (rdr.GetOrdinal ("visible"));
						be.Id = postid;
					}
				}
			}
			return be;
		}
		public override long RemoveComment (long cmtid)
		{
			long postid = 0;
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "delete from comment where _id = @id returning postid";
				cmd.Parameters.Add ("id", cmtid);
				cnx.Open ();
				postid = (long) cmd.ExecuteScalar ();
			}
			return postid;
		}
		public override BlogEntry GetPost (string username, string title)
		{
			BlogEntry be = null;
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {
				cmd.CommandText = "select _id,bcontent,modified,posted,visible from blog " +
				                  "where applicationname = @appname and username = @username and title = @title";
				cmd.Parameters.Add ("@appname", applicationName);
				cmd.Parameters.Add ("@username", username);
				cmd.Parameters.Add ("@title", title);
				cnx.Open ();
				using (NpgsqlDataReader rdr = cmd.ExecuteReader()) {
					if (rdr.Read ()) {
						be = new BlogEntry ();
						be.Title = title;
						be.Content = rdr.GetString (rdr.GetOrdinal ("bcontent"));
						be.UserName = username;
						be.Modified = rdr.GetDateTime (rdr.GetOrdinal ("modified"));
						be.Posted = rdr.GetDateTime (rdr.GetOrdinal ("posted"));
						be.Visible = rdr.GetBoolean (rdr.GetOrdinal ("visible"));
						be.Id = rdr.GetInt64 (rdr.GetOrdinal ("_id"));
					}
				}
			}
			return be;
		}

		public override long Post (string username, string title, string content, bool visible)
		{
			if (username == null)
				throw new ArgumentNullException("username");
			if (title == null)
				throw new ArgumentNullException("title");
			if (content == null)
				throw new ArgumentNullException("content");
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {
				cmd.CommandText = "insert into blog (title,bcontent,modified,posted,visible,username,applicationname)" +
				                  "values (@title,@bcontent,@modified,@posted,@visible,@username,@appname) returning _id";
				cmd.Parameters.Add ("@title", title);
				cmd.Parameters.Add ("@bcontent", content);
				DateTime now = DateTime.Now;
				cmd.Parameters.Add ("@modified", now);
				cmd.Parameters.Add ("@posted", now);
				cmd.Parameters.Add ("@visible", visible);
				cmd.Parameters.Add ("@username", username);
				cmd.Parameters.Add ("@appname", applicationName);
				cnx.Open ();
				return (long) cmd.ExecuteScalar();
			}
		}

		public override BlogEntryCollection FindPost (string pattern, FindBlogEntryFlags searchflags, int pageIndex, int pageSize, out int totalRecords)
		{
			BlogEntryCollection c = new BlogEntryCollection ();
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {
				cmd.CommandText = "select title,bcontent,modified,posted,username,visible from blog " +
					"where applicationname = @appname";
				cmd.Parameters.Add ("@appname", applicationName);
				if ((searchflags & FindBlogEntryFlags.MatchContent) > 0) {
					cmd.CommandText += " and bcontent like @bcontent";
					cmd.Parameters.Add ("@bcontent", pattern);
				}
				if ((searchflags & FindBlogEntryFlags.MatchTitle) > 0) {
					cmd.CommandText += " and title like @title";
					cmd.Parameters.Add ("@title", pattern);
				}
				if ((searchflags & FindBlogEntryFlags.MatchUserName) > 0) {
					cmd.CommandText += " and username like @username";
					cmd.Parameters.Add ("@username", pattern);
				}
				if ((searchflags & FindBlogEntryFlags.MatchInvisible) == 0) {
					cmd.CommandText += " and visible = true";
				}

				cmd.CommandText += " order by posted desc";
				cnx.Open ();
				using (NpgsqlDataReader rdr = cmd.ExecuteReader()) {
					totalRecords = 0;
					int firstrec = pageIndex * pageSize;
					int lastrec = firstrec + pageSize - 1;
					while (rdr.Read()) {
						if (totalRecords >= firstrec && totalRecords <= lastrec) {
							BlogEntry be = new BlogEntry ();
							be.Title = rdr.GetString (rdr.GetOrdinal ("title"));
							be.Content = rdr.GetString (rdr.GetOrdinal ("bcontent"));
							be.UserName = rdr.GetString (rdr.GetOrdinal ("username"));
							be.Posted = rdr.GetDateTime (rdr.GetOrdinal ("posted"));
							be.Modified = rdr.GetDateTime (rdr.GetOrdinal ("modified"));
							be.Visible =  rdr.GetBoolean (rdr.GetOrdinal ("visible"));
							c.Add (be);
						}
						totalRecords++;
					}
				}
			}
			return c;
		}

		public override void RemovePost (string username, string title)
		{
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand()) {
				cmd.CommandText = "delete from blog where username = @username and applicationname = @appname and title=@title";
				cmd.Parameters.Add ("@username",username);
				cmd.Parameters.Add ("@appname", applicationName);
				cmd.Parameters.Add ("@title",title);
				cnx.Open ();
				cmd.ExecuteNonQuery ();
				cnx.Close();
			}
		}


		int defaultPageSize = 10;

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
				                  "from blog where applicationname = @appname and visible = true " +
				                  " order by posted desc limit @len" ;

				cmd.Parameters.Add ("@appname", applicationName);
				cmd.Parameters.Add ("@len", defaultPageSize);
				cnx.Open ();
				using (NpgsqlDataReader rdr = cmd.ExecuteReader()) {
					totalRecords = 0;
					int firstrec = pageIndex * pageSize;
					int lastrec = firstrec + pageSize - 1;
					while (rdr.Read()) {
						if (totalRecords >= firstrec && totalRecords <= lastrec) {
							BlogEntry be = new BlogEntry ();
							be.Title = rdr.GetString (rdr.GetOrdinal ("title"));
							be.Content = rdr.GetString (rdr.GetOrdinal ("bcontent"));
							be.UserName = rdr.GetString (rdr.GetOrdinal ("username"));
							be.Posted = rdr.GetDateTime (rdr.GetOrdinal ("posted"));
							be.Modified = rdr.GetDateTime (rdr.GetOrdinal ("modified"));
							be.Visible = true; // because of sql code used
							c.Add (be);
						}
						totalRecords++;
					}
				}
			}
			return c;
		}
		#endregion
	}
}
