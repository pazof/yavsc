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
		/// Initialize the specified requestContext.
		/// </summary>
		/// <param name="requestContext">Request context.</param>
		[Authorize]
		protected override void Initialize (System.Web.Routing.RequestContext requestContext)
		{
			base.Initialize (requestContext);
		}

		/// <summary>
		/// Index this instance.
		/// </summary>
		[Authorize]
		public ActionResult Index (string id)
		{
			FileSystemManager fsmgr = new FileSystemManager ();
			var files = fsmgr.GetFiles (id);
			files.Owner = Membership.GetUser ().UserName;
			return View (files);
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
			FileSystemManager fsmgr = new FileSystemManager ();
			FileInfo fi = fsmgr.FileInfo (id);

			ViewData ["id"] = id;
			// TODO : ensure that we use the default port for 
			// the used sheme
			ViewData ["url"] = 
			string.Format(
					"{0}://{1}/users/{2}/{3}", 
					Request.Url.Scheme,
					Request.Url.Authority,
				 Membership.GetUser ().UserName ,
				 id );
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
			FileSystemManager fsmgr = new FileSystemManager ();
			return View ("Index",fsmgr.GetFiles(id));
		}

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