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
		private string sitename =
			WebConfigurationManager.AppSettings ["Name"];

		/// <summary>
		/// Index the specified title, pageIndex and pageSize.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		public ActionResult Index (string title, int pageIndex = 0, int pageSize = 10)
		{
			if (title != null)
				return Title (title, pageIndex, pageSize);
			
			return BlogList (pageIndex, pageSize);
		}
		/// <summary>
		/// Chooses the media.
		/// </summary>
		/// <returns>The media.</returns>
		/// <param name="id">Identifier.</param>
		public ActionResult ChooseMedia(long postid) 
		{
			return View ();
		}

		/// <summary>
		/// Blogs the list.
		/// </summary>
		/// <returns>The list.</returns>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		public ActionResult BlogList (int pageIndex = 0, int pageSize = 10)
		{
			int totalRecords;
			var bs = BlogManager.LastPosts (pageIndex, pageSize, out totalRecords);
			ViewData ["ResultCount"] = totalRecords; 
			ViewData ["PageSize"] = pageSize;
			ViewData ["PageIndex"] = pageIndex;
			var bec = new BlogEntryCollection (bs);
			return View ("Index", bec );
		}


		/// <summary>
		/// Title the specified title, pageIndex and pageSize.
		/// </summary>
		/// <param name="id">Title.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		[HttpGet]
		public ActionResult Title (string title, int pageIndex = 0, int pageSize = 10)
		{
			int recordCount;
			MembershipUser u = Membership.GetUser ();
			string username = u == null ? null : u.UserName;
			FindBlogEntryFlags sf = FindBlogEntryFlags.MatchTitle;
			BlogEntryCollection c = 
				BlogManager.FindPost (username, title, sf, pageIndex, pageSize, out recordCount);
			var utc = new UTBlogEntryCollection (title);
			utc.AddRange (c);
			ViewData ["ResultCount"] = recordCount;
			ViewData ["PageIndex"] = pageIndex; 
			ViewData ["PageSize"] = pageSize;
			return View ("Title", utc);
		}

		/// <summary>
		/// Users the posts.
		/// </summary>
		/// <returns>The posts.</returns>
		/// <param name="user">User.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		[HttpGet]
		public ActionResult UserPosts (string user, string title=null, int pageIndex = 0, int pageSize = 10)
		{
			if (user == null)
				return Index (title, pageSize, pageIndex);
			if (title != null) return UserPost (user, title, pageIndex, pageSize);
			int recordcount=0;
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
				BlogManager.FindPost (readersName, user, sf, pageIndex, pageSize, out recordcount);
			// Get author's meta data
			var pr = ProfileBase.Create (user);
			if (pr != null) {
				Profile bupr = new Profile (pr);
				// listing meta data
				ViewData ["BlogUserProfile"] = bupr;
				ViewData ["BlogTitle"] = bupr.BlogTitle;
				ViewData ["Avatar"] = bupr.avatar;
			}
			UUBlogEntryCollection uuc = new UUBlogEntryCollection (user, c);
			ViewData ["ResultCount"] = recordcount;
			ViewData ["PageIndex"] = pageIndex; 
			ViewData ["PageSize"] = pageSize;
			return View ("UserPosts", uuc);
		}

		/// <summary>
		/// Removes the comment.
		/// </summary>
		/// <returns>The comment.</returns>
		/// <param name="cmtid">Cmtid.</param>
		[Authorize(Roles="Blogger")]
		public ActionResult RemoveComment (long cmtid)
		{
			long postid = BlogManager.RemoveComment (cmtid);
			return GetPost (postid);
		}

		/// <summary>
		/// Gets the post.
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="postid">Postid.</param>
		public ActionResult GetPost (long postid)
		{
			ViewData ["id"] = postid;
			BlogEntry e = BlogManager.GetForReading (postid);
			UUTBlogEntryCollection c = new UUTBlogEntryCollection (e.Author,e.Title);
			c.Add (e);
			ViewData ["user"] = c.Author;
			ViewData ["title"] = c.Title;
			Profile pr = new Profile (ProfileBase.Create (c.Author));
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
		/// * bec.All(x=>x.Author == bec[0].Author) ;
		/// </summary>
		/// <returns>The post.</returns>
		/// <param name="bec">Bec.</param>
		private ActionResult UserPost (UUTBlogEntryCollection bec)
		{
			if (ModelState.IsValid)
			if (bec.Count > 0) {
				Profile pr = new Profile (ProfileBase.Create (bec.Author));
				if (pr == null)
					// the owner's profile must exist 
					// in order to publish its bills
					// This should'nt occur, as long as 
					// a profile must exist for each one of 
					// existing user record in data base
					// and each post is deleted with user deletion
					// a post => an author => a profile
					throw new Exception("Unexpected error retreiving author's profile");
				ViewData ["BlogUserProfile"] = pr;
				ViewData ["Avatar"] = pr.avatar;
				ViewData ["BlogTitle"] = pr.BlogTitle;
				MembershipUser u = Membership.GetUser ();

				ViewData ["Author"] = bec.Author;
				if (!pr.BlogVisible) {
					// only deliver to admins or owner
					if (u == null)
						return View ("NotAuthorized");
					else {
						if (u.UserName != bec.Author)
						if (!Roles.IsUserInRole (u.UserName, "Admin"))
							return View ("NotAuthorized");
					}
				}
				if (u == null || (u.UserName != bec.Author) && !Roles.IsUserInRole (u.UserName, "Admin")) {
					// Filer on allowed posts
					BlogEntryCollection filtered = bec.FilterFor((u == null)?null : u.UserName);
					UUTBlogEntryCollection nbec = new UUTBlogEntryCollection (bec.Author, bec.Title);
					nbec.AddRange (filtered);
					View ("UserPost",nbec);
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
		/// Post the specified title.
		/// </summary>
		/// <param name="title">Title.</param>
		[Authorize(Roles="Blogger")]
		public ActionResult Post (string title)
		{
			string un = Membership.GetUser ().UserName;
			if (String.IsNullOrEmpty (title))
				title = "";
			ViewData ["SiteName"] = sitename;
			ViewData ["Author"] = un;
			ViewData ["AllowedCircles"] = CircleManager.DefaultProvider.List (un)
				.Select (x => new SelectListItem {
				Value = x.Id.ToString(),
				Text = x.Title
			});

			return View ("Edit", new BlogEntry { Title = title, Author = un });
		}

		/// <summary>
		/// Validates the edit.
		/// </summary>
		/// <returns>The edit.</returns>
		/// <param name="model">Model.</param>
		[Authorize(Roles="")]
		public ActionResult Edit (BlogEntry model)
		{
			ViewData ["SiteName"] = sitename;
			ViewData ["Author"] = Membership.GetUser ().UserName;
			if (ModelState.IsValid) {
				if (model.Id != 0) {
					// ensures rights to update
					BlogManager.GetForEditing (model.Id, true);
					BlogManager.UpdatePost (model.Id, model.Title, model.Content, model.Visible, model.AllowedCircles);
					YavscHelpers.Notify (ViewData, LocalizedText.BillUpdated);

				} else {
					model.Id = BlogManager.Post (model.Author, model.Title, model.Content, model.Visible, model.AllowedCircles);
					YavscHelpers.Notify (ViewData, LocalizedText.BillCreated);
				}
				BlogManager.UpdatePostPhoto (model.Id, model.Photo);
			}
			ViewData ["AllowedCircles"] = 
				CircleManager.DefaultProvider.List (
					Membership.GetUser ().UserName).Select (x => new SelectListItem {
						Value = x.Id.ToString(),
						Text = x.Title,
						Selected = (model.AllowedCircles==null)? false : model.AllowedCircles.Contains (x.Id)
					});
			return View ("Edit", model);
		}

		/// <summary>
		/// Edit the specified bill 
		/// </summary>
		/// <param name="id">Identifier.</param>
		[Authorize(Roles="Blogger")]
		public ActionResult EditId (long postid)
		{
			
			BlogEntry e = BlogManager.GetForEditing (postid);
			string user = Membership.GetUser ().UserName;
			Profile pr = new Profile (ProfileBase.Create(e.Author));
			ViewData ["BlogTitle"] = pr.BlogTitle;
			ViewData ["LOGIN"] = user; 
			ViewData ["Id"] = postid;
			// Populates the circles combo items

			if (e.AllowedCircles == null)
				e.AllowedCircles = new long[0];
			
			ViewData ["AllowedCircles"] = 
				CircleManager.DefaultProvider.List (
					Membership.GetUser ().UserName).Select (x => new SelectListItem {
				Value = x.Id.ToString(),
				Text = x.Title,
				Selected = e.AllowedCircles.Contains (x.Id)
			});
			return View ("Edit",e);
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

		/// <summary>
		/// Remove the specified blog entry, by its author and title, 
		/// using returnUrl as the URL to return to,
		/// and confirm as a proof you really know what you do.
		/// </summary>
		/// <param name="id">Title.</param>
		/// <param name="user">User.</param>
		/// <param name="returnUrl">Return URL.</param>
		/// <param name="confirm">If set to <c>true</c> confirm.</param>
		[Authorize(Roles="Blogger")]
		public ActionResult RemoveTitle (string user, string title, string returnUrl, bool confirm = false)
		{
			if (returnUrl == null)
			if (Request.UrlReferrer != null)
				returnUrl = Request.UrlReferrer.AbsoluteUri;
			ViewData ["returnUrl"] = returnUrl;
			ViewData ["Author"] = user;
			ViewData ["Title"] = title;

			if (Membership.GetUser ().UserName != user)
			if (!Roles.IsUserInRole("Admin"))
				throw new AuthorizationDenied (user);
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
		[Authorize(Roles="Blogger")]
		public ActionResult RemovePost (long postid, string returnUrl, bool confirm = false)
		{
			// ensures the access control
			BlogEntry e = BlogManager.GetForEditing (postid);
			if (e == null)
				return new HttpNotFoundResult ("post id "+postid.ToString());
			ViewData ["id"] = postid;
			ViewData ["returnUrl"] = string.IsNullOrWhiteSpace(returnUrl)?
				Request.UrlReferrer.AbsoluteUri.ToString(): returnUrl;
			// TODO: cleaner way to disallow deletion
			if (!confirm)
				return View ("RemovePost",e);
			BlogManager.RemovePost (postid);
			if (string.IsNullOrWhiteSpace(returnUrl))
				return RedirectToAction ("Index");
			return Redirect (returnUrl);
		}
	}
}

