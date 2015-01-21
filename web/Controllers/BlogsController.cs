using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Profile;
using System.Web.Security;
using CodeKicker.BBCode;
using Npgsql.Web.Blog;
using Yavsc;
using Yavsc.Model;
using Yavsc.Model.Blogs;
using Yavsc.ApiControllers;
using Yavsc.Model.RolesAndMembers;
using System.Net;

namespace Yavsc.Controllers
{
	public class BlogsController : Controller
	{
		string defaultAvatarMimetype;
		private string sitename =
			WebConfigurationManager.AppSettings ["Name"];
		string avatarDir = "~/avatars";

		public string AvatarDir {
			get { return avatarDir; }
			set { avatarDir = value; }
		}

		public BlogsController ()
		{
			string[] defaultAvatarSpec = ConfigurationManager.AppSettings.Get ("DefaultAvatar").Split (';');
			if (defaultAvatarSpec.Length != 2)
				throw new ConfigurationErrorsException ("the DefaultAvatar spec should be found as <fileName>;<mime-type> ");
			defaultAvatar = defaultAvatarSpec [0];
			defaultAvatarMimetype = defaultAvatarSpec [1];
		}

		public ActionResult Index (string user = null, string title = null, int pageIndex=0, int pageSize=10)
		{
			if (string.IsNullOrEmpty (user)) {
				return BlogList (pageIndex, pageSize);
			} else {
				MembershipUser u = null;
				if (Membership.FindUsersByName (user) != null) 
					u= Membership.GetUser (user, false);
				if (u == null) {
					ModelState.AddModelError ("UserName",
						string.Format ("Utilisateur inconu : {0}", user));
					return BlogList ();
				} else {
					if (string.IsNullOrEmpty (title))
						return UserPosts (user, pageIndex, pageSize);
					return UserPost (user, title);
				}


			}
		}

		public ActionResult BlogList (int pageIndex = 0, int pageSize = 10)
		{
			ViewData ["SiteName"] = sitename;
			int totalRecords;
			BlogEntryCollection bs = BlogManager.LastPosts (pageIndex, pageSize, out totalRecords);
			ViewData ["RecordCount"] = totalRecords; 
			ViewData ["PageSize"] = pageSize;
			ViewData ["PageIndex"] = pageIndex;
			return View ("Index", bs);
		}

		[HttpGet]
		public ActionResult UserPosts (string user, int pageIndex = 0, int pageSize = 10)
		{
			int tr;
			MembershipUser u = Membership.GetUser ();
			FindBlogEntryFlags sf = FindBlogEntryFlags.MatchUserName;
			ViewData ["SiteName"] = sitename;
			ViewData ["BlogUser"] = user;
			if (u != null)
			if (u.UserName == user)
				sf |= FindBlogEntryFlags.MatchInvisible;
			BlogEntryCollection c = BlogManager.FindPost (user, sf, pageIndex, pageSize, out tr);
			Profile bupr =  AccountController.GetProfile (user);
			ViewData ["BlogUserProfile"] = bupr;
			ViewData ["BlogTitle"] = bupr.BlogTitle;
			ViewData ["PageIndex"] = pageIndex;
			ViewData ["PageSize"] = pageSize;
			ViewData ["RecordCount"] = tr; 
			return View ("UserPosts", c);
		
		}

		[Authorize]
		public ActionResult RemoveComment(long cmtid) 
		{
			long postid = BlogManager.RemoveComment (cmtid);
			return UserPost (postid);
		}

		private ActionResult UserPost (long id)
		{
			ViewData ["PostId"] = id;

			BlogEntry e = BlogManager.GetPost (id);
			return UserPost (e);
		}

		private ActionResult UserPost (BlogEntry e)
		{
			if (e == null)
				return View ("TitleNotFound");
			Profile pr = AccountController.GetProfile (e.UserName);
			if (pr==null)
				return View ("TitleNotFound");
			ViewData ["BlogUserProfile"] = pr;
			ViewData ["BlogTitle"] = pr.BlogTitle;
			ViewData ["HasAvatar"] = pr.avatar != null;
			MembershipUser u = Membership.GetUser ();
			if (u != null)
				ViewData ["UserName"] = u.UserName;
			if (!e.Visible || !pr.BlogVisible) {
				if (u==null)
					return View ("TitleNotFound");
				else {
					if (u.UserName!=e.UserName)
					if (!Roles.IsUserInRole(u.UserName,"Admin"))
					return View ("TitleNotFound");
				}
			}
			ViewData ["Comments"] = BlogManager.GetComments (e.Id);
			return View ("UserPost", e);
		}

		public ActionResult UserPost (string user, string title)
		{
			ViewData ["BlogUser"] = user;
			ViewData ["PostTitle"] = title;
			int postid = 0;
			if (string.IsNullOrEmpty (title)) {
				if (int.TryParse (user, out postid)) {
					return UserPost (BlogManager.GetPost (postid));
				}
			}
			string prevstr = LocalizedText.Preview;
			return UserPost (BlogManager.GetPost (user, title));
		}

		[Authorize]
		public ActionResult Post (string user, string title)
		{
			ViewData ["SiteName"] = sitename;
			string un = Membership.GetUser ().UserName;
			if (String.IsNullOrEmpty (user))
				user = un;
			ViewData ["UserName"] = un;
			return View (new BlogEditEntryModel { Title = title });
		}

		[Authorize]
		public ActionResult ValidatePost (BlogEditEntryModel model)
		{
			string username = Membership.GetUser ().UserName;
			ViewData ["SiteName"] = sitename;
			ViewData ["BlogUser"] = username;
			if (ModelState.IsValid) {
				if (!model.Preview) {
					BlogManager.Post (username, model.Title, model.Content, model.Visible);
					return UserPost (username, model.Title);
				}
			}
			return View ("Post", model);
		}

		[Authorize]
		public ActionResult ValidateEdit (BlogEditEntryModel model)
		{
			ViewData ["SiteName"] = sitename;
			ViewData ["BlogUser"] = Membership.GetUser ().UserName;
			if (ModelState.IsValid) {
				if (!model.Preview) {
					BlogManager.UpdatePost (model.Id, model.Title, model.Content, model.Visible);
					return UserPost (model);
				}
			}
			return View ("Edit", model);
		}

		[Authorize]
		public ActionResult Edit (BlogEditEntryModel model)
		{
			if (model != null) {
				string user = Membership.GetUser ().UserName;
				Profile pr = new Profile (HttpContext.Profile);

				ViewData ["BlogTitle"] = pr.BlogTitle;
				ViewData ["UserName"] = user;
				if (model.UserName == null) {
					model.UserName = user; 
					BlogEntry e = BlogManager.GetPost (model.UserName, model.Title);
					if (e == null) {
						return View ("TitleNotFound");
					} else {
						model = new BlogEditEntryModel (e);
						ModelState.Clear ();
						this.TryValidateModel (model);
					}
				} else if (model.UserName != user) {
					return View ("TitleNotFound");
				}
			}
			return View (model);
		}


		[Authorize]
		public ActionResult Comment (BlogEditCommentModel model) {
			string username = Membership.GetUser ().UserName;
			ViewData ["SiteName"] = sitename;
			if (ModelState.IsValid) {
				if (!model.Preview) {
					BlogManager.Comment(username, model.PostId, model.CommentText, model.Visible);
					return UserPost (model.PostId);
				}
			}
			return UserPost (model.PostId);
		}

		string defaultAvatar;

		[AcceptVerbs (HttpVerbs.Get)]
		public ActionResult Avatar (string user)
		{
			ProfileBase pr = ProfileBase.Create (user);
			string avpath = (string) pr.GetPropertyValue ("avatar");
			if (avpath==null) {
				FileInfo fia = new FileInfo (Server.MapPath (defaultAvatar));
				return File (fia.OpenRead (), defaultAvatarMimetype);
			}
			WebRequest wr = WebRequest.Create(avpath);
			FileContentResult res;
			using (WebResponse resp = wr.GetResponse ()) {
				using (Stream str = resp.GetResponseStream ()) {
					byte [] content = new byte[str.Length];
					str.Read (content, 0, (int) str.Length);
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
		public ActionResult RemovePost (string user, string title, string returnUrl, bool confirm=false)
		{
			if (returnUrl == null)
			if (Request.UrlReferrer!=null)
				returnUrl = Request.UrlReferrer.AbsoluteUri;
			ViewData["returnUrl"]=returnUrl;
			if (!confirm)
				return View ("RemovePost");
			BlogsApiController.RemovePost (user,title);
			if (returnUrl == null)
				RedirectToAction ("Index",new { user = user });
			return Redirect (returnUrl);
		}

		private ActionResult Return (string returnUrl)
		{
			if (!string.IsNullOrEmpty (returnUrl))
				return Redirect (returnUrl);
			else
				return RedirectToAction ("Index");
		}
	}
}

