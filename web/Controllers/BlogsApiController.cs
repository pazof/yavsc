using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Http;
using Npgsql.Web.Blog;
using Yavsc.Model.Blogs;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Blogs API controller.
	/// </summary>
	public class BlogsApiController : ApiController
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
			BlogEntry e = BlogManager.GetPost (postid);
			if (!Roles.IsUserInRole ("Admin")) {
				string rguser = Membership.GetUser ().UserName;
				if (rguser != e.UserName) {
					throw new AccessViolationException (
						string.Format (
							"Vous n'avez pas le droit de tagger des billets du Blog de {0}",
							e.UserName));
				}
			}
			return BlogManager.Tag (postid, tag);
		}
		/// <summary>
		/// Removes the post.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="title">Title.</param>
		public static void RemovePost(string user, string title) {
			if (!Roles.IsUserInRole ("Admin")) {
				string rguser = Membership.GetUser ().UserName;
				if (rguser != user) {
					throw new AccessViolationException (
						string.Format (
							"Vous n'avez pas le droit de suprimer des billets du Blog de {0}",
							user));
				}
			}
			BlogEntry e = BlogManager.GetPost (user, title);
			if (e == null) {
				throw new KeyNotFoundException (
					string.Format("Aucun post portant le titre \"{0}\" pour l'utilisateur {1}",
						title, user));
			}
			BlogManager.RemovePost (user, title);
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

