using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Security;
using System.Text.RegularExpressions;

namespace Yavsc.Controllers
{
	/// <summary>
	/// File system controller.
	/// </summary>
    public class FileSystemController : Controller
    {
		private static string usersDir ="~/users";
		/// <summary>
		/// Gets the users dir.
		/// </summary>
		/// <value>The users dir.</value>
		public static string UsersDir {
			get {
				return usersDir;
			}
		}
		/// <summary>
		/// Index this instance.
		/// </summary>
		[Authorize]
		public ActionResult Index()
        {
			string user = Membership.GetUser ().UserName;
			ViewData ["UserName"] = user;

			DirectoryInfo di = new DirectoryInfo (
				Path.Combine(
					Server.MapPath(UsersDir),
					user));
			if (!di.Exists)
				di.Create ();
			return View (new FileInfoCollection( di.GetFiles()));
        }
		/// <summary>
		/// Details the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public ActionResult Details(string id)
        {
			foreach (char x in Path.GetInvalidPathChars()) {
				if (id.Contains (x)) {
					ViewData ["Message"] =
					string.Format (
							"Something went wrong following the following path : {0} (\"{1}\")",
							id,x);
					return RedirectToAction ("Index");
				}
			}
			string fpath = Path.Combine (BaseDir, id);
			ViewData["Content"] = Url.Content (fpath);
			FileInfo fi = new FileInfo (fpath);

			return View (fi);
        }
		/// <summary>
		/// Create this instance.
		/// </summary>
        public ActionResult Create()
        {
            return View ();
        } 

		/// <summary>
		/// Create the specified collection.
		/// </summary>
		/// <param name="collection">Collection.</param>
		[HttpPost]
		[Authorize]
		public ActionResult Create(FormCollection collection)
		{
			try {
				string fnre = "[A-Za-z0-9~\\-.]+";
				HttpFileCollectionBase hfc = Request.Files;

				for (int i=0; i<hfc.Count; i++)
				{
					if (!Regex.Match(hfc[i].FileName,fnre).Success)
					{
						ViewData ["Message"] += string.Format("<p>File name '{0}' refused</p>",hfc[i].FileName);
						ModelState.AddModelError(
							"AFile",
							string.Format(
								"The file name {0} dosn't match an acceptable file name {1}",
								hfc[i].FileName,fnre))
						;
						return View();
					}
				}
				for (int i=0; i<hfc.Count; i++)
					{
					// TODO Limit with hfc[h].ContentLength
					string filename = Path.Combine(Server.MapPath(BaseDir),hfc[i].FileName);
					hfc[i].SaveAs(filename);
					ViewData ["Message"] += string.Format("<p>File name '{0}' saved</p>",hfc[i].FileName);
				}
				return RedirectToAction ("Index","FileSystem");
			} catch (Exception e) {
				ViewData ["Message"] = "Exception:"+e.Message;
				return View ();
			}
		}

		/// <summary>
		/// Gets the base dir.
		/// </summary>
		/// <value>The base dir.</value>
		public static string BaseDir { get { return Path.Combine (UsersDir, Membership.GetUser ().UserName); } }
		/// <summary>
		/// Edit the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
        public ActionResult Edit(int id)
        {
            return View ();
        }
		/// <summary>
		/// Edit the specified id and collection.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="collection">Collection.</param>
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try {
                return RedirectToAction ("Index");
            } catch {
                return View ();
            }
        }
		/// <summary>
		/// Delete the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
        public ActionResult Delete(int id)
        {
            return View ();
        }
		/// <summary>
		/// Delete the specified id and collection.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="collection">Collection.</param>
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try {
                return RedirectToAction ("Index");
            } catch {
                return View ();
            }
        }
    }
}