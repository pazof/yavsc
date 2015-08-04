using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Http;
using Npgsql.Web.Blog;
using Yavsc.Model.Blogs;
using System.IO;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Blogs API controller.
	/// </summary>
	public class BlogsController : ApiController
	{
		private const string adminRoleName = "Admin";

		/// <summary>
		/// Initialize the specified controllerContext.
		/// </summary>
		/// <param name="controllerContext">Controller context.</param>
		protected override void Initialize (System.Web.Http.Controllers.HttpControllerContext controllerContext)
		{
			base.Initialize (controllerContext);
			if (!Roles.RoleExists (adminRoleName)) {
				Roles.CreateRole (adminRoleName);
			}
		}

		/// <summary>
		/// Tag the specified postid and tag.
		/// </summary>
		/// <param name="postid">Postid.</param>
		/// <param name="tag">Tag.</param>
		public long Tag (long postid,string tag) {
			BlogManager.GetForEditing (postid);
			return BlogManager.Tag (postid, tag);
		}

		/// <summary>
		/// Removes the post.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="title">Title.</param>
		[Authorize]
		public void RemoveTitle(string user, string title) {
			BlogManager.CheckAuthCanEdit (user,title);
			BlogManager.RemoveTitle (user, title);
		}
		/// <summary>
		/// Removes the tag.
		/// </summary>
		/// <param name="tagid">Tagid.</param>
		public void RemoveTag(long tagid) {
			
			throw new NotImplementedException ();
		}
	}
}

