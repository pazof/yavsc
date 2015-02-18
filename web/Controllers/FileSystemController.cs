using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Security;
using System.Text.RegularExpressions;
using Yavsc.Model.FileSystem;

namespace Yavsc.Controllers
{
	/// <summary>
	/// File system controller.
	/// </summary>
	public class FileSystemController : Controller
	{
		/// <summary>
		/// Gets the users base directory.
		/// </summary>
		/// <value>The users dir.</value>
		public string RootDir {
			get {
				return mgr.Prefix;
			}
		}

		FileSystemManager mgr = null ;

		/// <summary>
		/// Initialize the specified requestContext.
		/// </summary>
		/// <param name="requestContext">Request context.</param>
		protected override void Initialize (System.Web.Routing.RequestContext requestContext)
		{
			base.Initialize (requestContext);
			mgr = new FileSystemManager (
				string.Format("~/users/{0}",Membership.GetUser().UserName));
		}

		/// <summary>
		/// Index this instance.
		/// </summary>
		[Authorize]
		public ActionResult Index (string id)
		{
			return View (mgr.GetFiles (id));
		}

		/// <summary>
		/// Details the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public ActionResult Details (string id)
		{
			foreach (char x in Path.GetInvalidPathChars()) {
				if (id.Contains (x)) {
					ViewData ["Message"] =
					string.Format (
						"Something went wrong following the following path : {0} (\"{1}\")",
						id, x);
					return RedirectToAction ("Index");
				}
			}
			FileInfo fi = mgr.FileInfo (id);

			ViewData ["Content"] = Url.Content (fi.FullName);

			return View (fi);
		}

		/// <summary>
		/// Create the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		[HttpPost]
		[Authorize]
		public ActionResult Create (string id)
		{
			mgr.Put ( id, Request.Files);
			return View ("Index",mgr.GetFiles(id));
		}

		/// <summary>
		/// Gets the user's base dir.
		/// </summary>
		/// <value>The base dir.</value>
		public string UserBaseDir { get { return Path.Combine (RootDir, Membership.GetUser ().UserName); } }

		/// <summary>
		/// Edit the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public ActionResult Edit (int id)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Edit the specified id and collection.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="collection">Collection.</param>
		[HttpPost]
		public ActionResult Edit (int id, FormCollection collection)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Delete the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public ActionResult Delete (int id)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Delete the specified id and collection.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="collection">Collection.</param>
		[HttpPost]
		public ActionResult Delete (int id, FormCollection collection)
		{
			throw new NotImplementedException ();
		}
	}
}