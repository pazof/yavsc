using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Security;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Model.Admin;
using Yavsc.Admin;
using System.IO;
using Yavsc.Model;


namespace Yavsc.Controllers
{
	/// <summary>
	/// Admin controller.
	/// Only Admin members should be allowed to use it.
	/// </summary>
	public class AdminController : Controller
	{
		public ActionResult Index()
		{
			if (!Roles.RoleExists (adminRoleName)) {
				Roles.CreateRole (adminRoleName);
			}
			return View ();
		}

		public ActionResult InitDb(DataAccess datac, string doInit)
		{
			if (doInit=="on") {
				if (ModelState.IsValid) {
					// TODO BETTER
					datac.BackupPrefix = Server.MapPath (datac.BackupPrefix);
					DataManager mgr = new DataManager (datac);
					TaskOutput t = mgr.CreateDb ();
					return View ("Created", t);
				}
			}
			return View ();
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
					datac.BackupPrefix = Server.MapPath (datac.BackupPrefix);
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
				// TODO BETTER
				datac.BackupPrefix = Server.MapPath (datac.BackupPrefix);
				DataManager mgr = new DataManager (datac);
				ViewData ["BackupName"] = backupName;
				ViewData ["DataOnly"] = dataOnly;

				TaskOutput t = mgr.Restore (
					Path.Combine(new FileInfo(datac.BackupPrefix).DirectoryName,
						backupName),dataOnly);
				return View ("Restored", t);
			}
			BuildBackupList (datac);
			return View (datac);
		}
		private void BuildBackupList(DataAccess datac)
		{
			// build ViewData ["Backups"];
			string bckd=Server.MapPath (datac.BackupPrefix);
			DirectoryInfo di = new DirectoryInfo (new FileInfo(bckd).DirectoryName);
			List<string> bks = new List<string> ();
			foreach (FileInfo ti in di.GetFiles("*.tar"))
				bks.Add (ti.Name);
			ViewData ["Backups"] = bks.ToArray ();
		}

		[Authorize(Roles="Admin")]
		public ActionResult RemoveFromRole(string username, string rolename, string returnUrl)
		{
			Roles.RemoveUserFromRole(username,rolename);
			return Redirect(returnUrl);
		}

		[Authorize(Roles="Admin")]
		public ActionResult RemoveUser (string username, string submitbutton)
		{
			if (submitbutton == "Supprimer") {
				Membership.DeleteUser (username);
				ViewData["Message"]=
					string.Format("utilisateur \"{0}\" supprimé",username);
			}
			return RedirectToAction("UserList");
		}
		[Authorize(Roles="Admin")]
		public ActionResult RemoveRole (string rolename, string submitbutton)
		{
			if (submitbutton == "Supprimer") 
			{
				Roles.DeleteRole(rolename);
			}
			return RedirectToAction("RoleList");
		}

		[Authorize(Roles="Admin")]
		public ActionResult RemoveRoleQuery(string rolename)
		{
			ViewData["roletoremove"] = rolename;
			return View ();
		}

		[Authorize(Roles="Admin")]
		public ActionResult RemoveUserQuery(string username)
		{
			ViewData["usertoremove"] = username;
			return UserList();
		}


		//TODO no more than pageSize results per page
		/// <summary>
		/// User list.
		/// </summary>
		/// <returns>The list.</returns>
		[Authorize()]
		public ActionResult UserList ()
		{
			MembershipUserCollection c = Membership.GetAllUsers ();
			return View (c);
		}

		/// <summary>
		/// a form to add a role
		/// </summary>
		/// <returns>The role.</returns>
		[Authorize(Roles="Admin")]
		public ActionResult AddRole ()
		{
			return View ();
		}

		/// <summary>
		/// Add a new role.
		/// </summary>
		/// <returns>The add role.</returns>
		/// <param name="rolename">Rolename.</param>
		[Authorize(Roles="Admin")]
		public ActionResult DoAddRole (string rolename)
		{
			Roles.CreateRole(rolename);
			ViewData["Message"] = LocalizedText.role_created+ " : "+rolename;
			return View ();
		}

		/// <summary>
		/// Shows the roles list.
		/// </summary>
		/// <returns>The list.</returns>
		[Authorize()]
		public ActionResult RoleList ()
		{
			return View (Roles.GetAllRoles ());
		}
		private const string adminRoleName = "Admin";



		/// <summary>
		/// Assing the Admin role to the specified user in model.
		/// </summary>
		/// <param name="model">Model.</param>
		[Authorize()]
		public ActionResult Admin (NewAdminModel model)
		{
			string currentUser = Membership.GetUser ().UserName;
			if (ModelState.IsValid) {
				Roles.AddUserToRole (model.UserName, adminRoleName);
				ViewData ["Message"] = model.UserName + " "+LocalizedText.was_added_to_the_role+" '" + adminRoleName + "'";
			} else {
				// ASSERT (Roles.RoleExists (adminRoleName)) 
				string [] admins = Roles.GetUsersInRole (adminRoleName);
				if (admins.Length > 0) { 
					if (! admins.Contains (Membership.GetUser ().UserName)) {
						ModelState.Remove("UserName");
						ModelState.AddModelError("UserName",LocalizedText.younotadmin+"!");
						return View ("Index");
					}
				} else {
					Roles.AddUserToRole (currentUser, adminRoleName);
					admins = new string[] { currentUser };
					ViewData ["Message"] += string.Format (
						LocalizedText.was_added_to_the_empty_role,
						currentUser, adminRoleName);
				}

				List<SelectListItem> users = new List<SelectListItem> ();
				foreach (MembershipUser u in Membership.GetAllUsers ()) {
					var i = new SelectListItem ();
					i.Text = string.Format ("{0} <{1}>", u.UserName, u.Email);
					i.Value = u.UserName;
					users.Add (i);
				}
				ViewData ["useritems"] = users;
				ViewData ["admins"] = admins;
			}
			return View (model);
		}
	}
}

