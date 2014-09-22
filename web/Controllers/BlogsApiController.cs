using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Security;
using Npgsql.Web.Blog;
using yavscModel.Blogs;

namespace Yavsc.Controllers
{
	public class BlogsApiController : Controller
	{
		public static HttpStatusCodeResult RemovePost(string user, string title) {
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
				return new HttpNotFoundResult (
					string.Format("Aucun post portant le titre \"{0}\" pour l'utilisateur {1}",
						title, user));
			}
			BlogManager.RemovePost (user, title);
			return new HttpStatusCodeResult (200);
		}
	}
}

