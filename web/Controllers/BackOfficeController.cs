using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yavsc.Admin;
using yavscModel.Admin;


namespace Yavsc.Controllers
{
    public class BackOfficeController : Controller
    {
		[Authorize(Roles="Admin")]
		public ActionResult Index(DataAccess model)
		{
			return View (model);
		}
		[Authorize(Roles="Admin")]
		public ActionResult Backups(DataAccess model)
		{
			return View (model);
		}

		[Authorize(Roles="Admin")]
		public ActionResult CreateBackup(DataAccess datac)
		{
			if (datac != null) {
				if (ModelState.IsValid) {
					if (string.IsNullOrEmpty (datac.Password))
						ModelState.AddModelError ("Password", "Invalid passord");
					DataManager ex = new DataManager (datac);
					Export e = ex.CreateBackup ();
					if (e.ExitCode > 0)
						ModelState.AddModelError ("Password", "Operation Failed");
					return View ("BackupCreated", e);
				}
			} else {
				datac = new DataAccess ();
			}
			return View (datac);
		}
		
		[Authorize(Roles="Admin")]
		public ActionResult CreateUserBackup(DataAccess datac,string username)
		{
			throw new NotImplementedException();
		}

		[Authorize(Roles="Admin")]
		public ActionResult Upgrade(DataAccess datac) {
			throw new NotImplementedException();
		}

		[Authorize(Roles="Admin")]
		public ActionResult Restore(DataAccess datac,string backupName,bool dataOnly=true)
		{
			ViewData ["BackupName"] = backupName;
			if (ModelState.IsValid) {
				DataManager mgr = new DataManager (datac);
				ViewData ["BackupName"] = backupName;
				ViewData ["DataOnly"] = dataOnly;
				TaskOutput t = mgr.Restore (backupName,dataOnly);
				return View ("Restored", t);
			}
			return View (datac);
		}

    }
}
