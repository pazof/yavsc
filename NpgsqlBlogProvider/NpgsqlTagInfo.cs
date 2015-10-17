//
//  NpgsqlTagInfo.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015 GNU GPL
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using Yavsc.Model.Blogs;
using System.Collections.Generic;

namespace Npgsql.Web.Blog
{
	/// <summary>
	/// Npgsql tag info.
	/// </summary>
	public class NpgsqlTagInfo: TagInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Npgsql.Web.Blog.NpgsqlTagInfo"/> class.
		/// </summary>
		/// <param name="connectionString">Connection string.</param>
		/// <param name="tagname">Tagname.</param>
		public NpgsqlTagInfo (string connectionString, string tagname): base(tagname)
		{
			titles = new List<BasePostInfo>();
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "SELECT \n" +
					"  blog.username, \n" +
					"  blog.posted, \n" +
					"  blog.modified, \n" +
					"  blog.title, \n" +
					"  blog.bcontent, \n" +
					"  blog.visible, \n" +
					"  blog._id, \n" +
					"  blog.photo, \n" +
					"  tag.name\nFROM \n" +
					"  public.blog, \n" +
					"  public.tagged, \n" +
					"  public.tag\nWHERE \n" +
					"  tagged.postid = blog._id AND \n" +
					"  tag._id = tagged.tagid AND \n" +
					" public.tag.name = :name";
				cmd.Parameters.AddWithValue ("name", tagname);

				using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
					while (rdr.Read ()) {
						bool truncated;
						var pi = new  BasePostInfo { 
							Title = rdr.GetString(3),
							Author = rdr.GetString (0), 
							Id =  rdr.GetInt64 (6), 
							Intro = MarkdownHelper.MarkdownIntro (
								rdr.GetString (4),
								out truncated),
							Visible = rdr.GetBoolean(5),
							Photo = (!rdr.IsDBNull (7))?null:rdr.GetString (7),
							Modified = rdr.GetDateTime(2),
							Posted = rdr.GetDateTime(1)
						};
						titles.Add (pi);
					}
				}
			}
		}

		List<BasePostInfo> titles;

		#region implemented abstract members of TagInfo
		/// <summary>
		/// Gets the titles.
		/// </summary>
		/// <value>The titles.</value>
		public override System.Collections.Generic.IEnumerable<BasePostInfo> Titles {
			get {
				return titles;
			}
		}
		#endregion
	}
}

