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
		public ActionResult Index (string user, string filename)
		{
			WebFileSystemManager fsmgr = new WebFileSystemManager ();
			var files = fsmgr.GetFiles (user,filename);
			return View (files);
		}

		/// <summary>
		/// Post the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public ActionResult Post (string id)
		{
			return View ();
		}

		/// <summary>
		/// Details the specified user and filename.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="filename">Filename.</param>
		public ActionResult Details (string user, string filename)
		{
			WebFileSystemManager fsmgr = new WebFileSystemManager ();
			FileInfo fi = fsmgr.FileInfo (filename);

			ViewData ["filename"] = filename;
			// TODO : ensure that we use the default port for 
			// the used sheme
			ViewData ["url"] = Url.Content("~/users/"+user+"/"+filename);
			return View (fi);
		}
	}
}