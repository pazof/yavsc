using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Configuration;
using System.Web.Profile;
using System.Web.Security;
using Npgsql.Web.Blog;
using Yavsc;
using Yavsc.Model;
using Yavsc.Model.Blogs;
using Yavsc.ApiControllers;
using Yavsc.Model.RolesAndMembers;
using System.Net;
using System.Web.Mvc;
using Yavsc.Model.Circles;
using Yavsc.Helpers;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Blogs controller.
	/// </summary>
	public class BlogsController : Controller
	{
		string defaultAvatarMimetype;
		private string sitename =
			WebConfigurationManager.AppSettings ["Name"];
		string avatarDir = "~/avatars";

		/// <summary>
		/// Gets or sets the avatar dir.
		/// </summary>
		/// <value>The avatar dir.</value>
		public string AvatarDir {
			get { return avatarDir; }
			set { avatarDir = value; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Controllers.BlogsController"/> class.
		/// </summary>
		public BlogsController ()
		{
			string[] defaultAvatarSpec = ConfigurationManager.AppSettings.Get ("DefaultAvatar").Split (';');
			if (defaultAvatarSpec.Length != 2)
				throw new ConfigurationErrorsException ("the DefaultAvatar spec should be found as <fileName>;<mime-type> ");
			defaultAvatar = defaultAvatarSpec [0];
			defaultAvatarMimetype = defaultAvatarSpec [1];
		}

		/// <summary>
		/// Index the specified user, title, pageIndex and pageSize.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="title">Title.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		public ActionResult Index (string user = null, string title = null, int pageIndex = 0, int pageSize = 10)
		{
			if (string.IsNullOrEmpty (user)) {
				return BlogList (pageIndex, pageSize);
			} else {
				MembershipUser u = null;
				if (Membership.FindUsersByName (user) != null)
					u = Membership.GetUser (user, false);
				if (u == null) {
					ModelState.AddModelError ("UserName",
						string.Format ("Utilisateur inconu : {0}", user));
					return BlogList ();
				} else {
					if (string.IsNullOrEmpty (title))
						return UserPosts (user, pageIndex, pageSize);
					return UserPost (user, title, pageIndex, pageSize);
				}
			}
		}

		/// <summary>
		/// Blogs the list.
		/// </summary>
		/// <returns>The list.</returns>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		public ActionResult BlogList (int pageIndex = 0, int pageSize = 10)
		{
			ViewData ["SiteName"] = sitename;
			int totalRecords;
			var bs = BlogManager.LastPosts (pageIndex, pageSize, out totalRecords);
			ViewData ["RecordCount"] = totalRecords; 
			ViewData ["pageSize"] = pageSize;
			ViewData ["PageIndex"] = pageIndex;
			return View ("Index", new BlogEntryCollection(bs) );
		}

		/// <summary>
		/// Users the posts.
		/// </summary>
		/// <returns>The posts.</returns>
		/// <param name="user">User.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		[HttpGet]
		public ActionResult UserPosts (string user, int pageIndex = 0, int pageSize = 10)
		{
			int tr;
			MembershipUser u = Membership.GetUser ();
			FindBlogEntryFlags sf = FindBlogEntryFlags.MatchUserName;
			ViewData ["SiteName"] = sitename;
			ViewData ["BlogUser"] = user;
			string readersName = null;
			ViewData ["PageIndex"] = pageIndex;
			ViewData ["pageSize"] = pageSize;
			// displays invisible items when the logged user is also the author
			if (u != null) {
				if (u.UserName == user || Roles.IsUserInRole ("Admin"))
					sf |= FindBlogEntryFlags.MatchInvisible;
				readersName = u.UserName;
			}
			// find entries
			BlogEntryCollection c = 
				BlogManager.FindPost (readersName, user, sf, pageIndex, pageSize, out tr);
			// Get author's meta data
			Profile bupr = new Profile (ProfileBase.Create (user));
			ViewData ["BlogUserProfile"] = bupr;
			// Inform of listing meta data
			ViewData ["BlogTitle"] = bupr.BlogTitle;
			ViewData ["Avatar"] = bupr.avatar;
			ViewData ["RecordCount"] = tr; 
			UUBlogEntryCollection uuc = new UUBlogEntryCollection (user, c);
			return View ("UserPosts", uuc);
		}

		/// <summary>
		/// Removes the comment.
		/// </summary>
		/// <returns>The comment.</returns>
		/// <param name="cmtid">Cmtid.</param>
		[Authorize]
		public ActionResult RemoveComment (long cmtid)
		{
			long postid = BlogManager.RemoveComment (cmtid);
			return GetPost (postid);
		}

		/// <summary>
		/// Returns the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="id">Identifier.</param>
		public ActionResult GetPost (long id)
		{
			ViewData ["id"] = id;
			BlogEntry e = BlogManager.GetForReading (id);
			UUTBlogEntryCollection c = new UUTBlogEntryCollection (e.UserName,e.Title);
			c.Add (e);
			ViewData ["user"] = c.UserName;
			ViewData ["title"] = c.Title;
			Profile pr = new Profile (ProfileBase.Create (c.UserName));
			if (pr == null)
				// the owner's profile must exist 
				// in order to publish its bills
				return View ("NotAuthorized");
			ViewData ["BlogUserProfile"] = pr;
			ViewData ["Avatar"] = pr.avatar;
			ViewData ["BlogTitle"] = pr.BlogTitle;
			return View ("UserPost",c);
		}

		/// <summary>
		/// Users the post.
		/// Assume that :
		/// * bec.Count > O
		/// * bec.All(x=>x.UserName == bec[0].UserName) ;
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="bec">Bec.</param>
		private ActionResult UserPost (UUTBlogEntryCollection bec)
		{
			if (ModelState.IsValid)
			if (bec.Count > 0) {
				Profile pr = new Profile (ProfileBase.Create (bec.UserName));
				if (pr == null)
					// the owner's profile must exist 
					// in order to publish its bills
					return View ("NotAuthorized");
				ViewData ["BlogUserProfile"] = pr;
				ViewData ["Avatar"] = pr.avatar;
				ViewData ["BlogTitle"] = pr.BlogTitle;
				MembershipUser u = Membership.GetUser ();
				if (u != null)
					ViewData ["UserName"] = u.UserName;
				if (!pr.BlogVisible) {
					// only deliver to admins or owner
					if (u == null)
						return View ("NotAuthorized");
					else {
						if (u.UserName != bec.UserName)
						if (!Roles.IsUserInRole (u.UserName, "Admin"))
							return View ("NotAuthorized");
					}
				}
			}
			return View ("UserPost",bec);
		}

		/// <summary>
		/// Users the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="user">User.</param>
		/// <param name="title">Title.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		public ActionResult UserPost (string user, string title, int pageIndex = 0, int pageSize = 10)
		{
			ViewData ["user"] = user;
			ViewData ["title"] = title;
			ViewData ["PageIndex"] = pageIndex;
			ViewData ["pageSize"] = pageSize;
			var pb = ProfileBase.Create (user);
			if (pb == null)
				// the owner's profile must exist 
				// in order to publish its bills
				return View ("NotAuthorized");
			Profile pr = new Profile (pb);
			ViewData ["BlogUserProfile"] = pr;
			ViewData ["Avatar"] = pr.avatar;
			ViewData ["BlogTitle"] = pr.BlogTitle;
			UUTBlogEntryCollection c = new UUTBlogEntryCollection (user, title);
			c.AddRange ( BlogManager.FilterOnReadAccess (BlogManager.GetPost (user, title)));
			return View ("UserPost",c);
		}

		/// <summary>
		/// Post the specified user and title.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="title">Title.</param>
		[Authorize,
		ValidateInput (false)]
		public ActionResult Post (string user, string title)
		{
			ViewData ["BlogUser"] = user;
			ViewData ["PostTitle"] = title;
			ViewData ["SiteName"] = sitename;
			string un = Membership.GetUser ().UserName;
			if (String.IsNullOrEmpty (user))
				user = un;
			if (String.IsNullOrEmpty (title))
				title = "";
			ViewData ["UserName"] = un;
			ViewData ["AllowedCircles"] = CircleManager.DefaultProvider.List (Membership.GetUser ().UserName).Select (x => new SelectListItem {
				Value = x.Id.ToString(),
				Text = YavscHelpers.FormatCircle(x).ToHtmlString()
			});

			return View ("Edit", new BlogEntry { Title = title });
		}

		/// <summary>
		/// Validates the edit.
		/// </summary>
		/// <returns>The edit.</returns>
		/// <param name="model">Model.</param>
		[Authorize,
			ValidateInput (false)]
		public ActionResult ValidateEdit (BlogEntry model)
		{
			ViewData ["SiteName"] = sitename;
			ViewData ["BlogUser"] = Membership.GetUser ().UserName;
			if (ModelState.IsValid) {
				if (model.Id != 0)
					BlogManager.UpdatePost (model.Id, model.Title, model.Content, model.Visible, model.AllowedCircles);
				else
					model.Id = BlogManager.Post (model.UserName, model.Title, model.Content, model.Visible, model.AllowedCircles);
				return RedirectToAction ("UserPosts", new { user = model.UserName, title = model.Title });
			}
			return View ("Edit", model);
		}

		/// <summary>
		/// Edit the specified bill 
		/// </summary>
		/// <param name="id">Identifier.</param>
		[Authorize, ValidateInput (false)]
		public ActionResult Edit (long id)
		{
			
			BlogEntry e = BlogManager.GetForEditing (id);
			string user = Membership.GetUser ().UserName;
			Profile pr = new Profile (ProfileBase.Create(e.UserName));
			ViewData ["BlogTitle"] = pr.BlogTitle;
			ViewData ["LOGIN"] = user; 
			// Populates the circles combo items

			if (e.AllowedCircles == null)
				e.AllowedCircles = new long[0];
			
			ViewData ["AllowedCircles"] = CircleManager.DefaultProvider.List (Membership.GetUser ().UserName).Select (x => new SelectListItem {
				Value = x.Id.ToString(),
				Text = x.Title,
				Selected = e.AllowedCircles.Contains (x.Id)
			});
			return View (e);
		}

		/// <summary>
		/// Comment the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		[Authorize]
		public ActionResult Comment (Comment model)
		{
			string username = Membership.GetUser ().UserName;
			ViewData ["SiteName"] = sitename;
			if (ModelState.IsValid) {
				BlogManager.Comment (username, model.PostId, model.CommentText, model.Visible);
				return GetPost (model.PostId);
			}
			return GetPost (model.PostId);
		}

		string defaultAvatar;

		/// <summary>
		/// Avatar the specified user.
		/// </summary>
		/// <param name="user">User.</param>
		[AcceptVerbs (HttpVerbs.Get)]
		public ActionResult Avatar (string user)
		{
			ProfileBase pr = ProfileBase.Create (user);
			string avpath = (string)pr.GetPropertyValue ("avatar");
			if (avpath == null) {
				FileInfo fia = new FileInfo (Server.MapPath (defaultAvatar));
				return File (fia.OpenRead (), defaultAvatarMimetype);
			}
			if (avpath.StartsWith ("~/")) {

			}
			WebRequest wr = WebRequest.Create (avpath);
			FileContentResult res;
			using (WebResponse resp = wr.GetResponse ()) {
				using (Stream str = resp.GetResponseStream ()) {
					byte[] content = new byte[str.Length];
					str.Read (content, 0, (int)str.Length);
					res = File (content, resp.ContentType);
					wr.Abort ();
					return res;
				}
			}
		}

		/// <summary>
		/// Remove the specified blog entry, by its author and title, 
		/// using returnUrl as the URL to return to,
		/// and confirm as a proof you really know what you do.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="title">Title.</param>
		/// <param name="returnUrl">Return URL.</param>
		/// <param name="confirm">If set to <c>true</c> confirm.</param>
		[Authorize]
		public ActionResult RemoveTitle (string user, string title, string returnUrl, bool confirm = false)
		{
			if (returnUrl == null)
			if (Request.UrlReferrer != null)
				returnUrl = Request.UrlReferrer.AbsoluteUri;
			ViewData ["returnUrl"] = returnUrl;
			ViewData ["UserName"] = user;
			ViewData ["Title"] = title;
			BlogManager.CheckAuthCanEdit (user, title);
			if (!confirm)
				return View ("RemoveTitle");
			BlogManager.RemoveTitle (user, title);
			if (returnUrl == null)
				RedirectToAction ("Index", new { user = user });
			return Redirect (returnUrl);
		}

		/// <summary>
		/// Removes the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="returnUrl">Return URL.</param>
		/// <param name="confirm">If set to <c>true</c> confirm.</param>
		[Authorize]
		public ActionResult RemovePost (long id, string returnUrl, bool confirm = false)
		{
			BlogEntry e = BlogManager.GetForEditing (id);
			if (e == null)
				return new HttpNotFoundResult ("post id "+id.ToString());
			ViewData ["id"] = id;
			ViewData ["returnUrl"] = string.IsNullOrWhiteSpace(returnUrl)?
				Request.UrlReferrer.AbsoluteUri.ToString(): returnUrl;
			// TODO: cleaner way to disallow deletion
			if (!confirm)
				return View ("RemovePost",e);
			BlogManager.RemovePost (id);
			if (string.IsNullOrWhiteSpace(returnUrl))
				return RedirectToAction ("Index");
			return Redirect (returnUrl);
		}
	}
}

