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
using Yavsc.Helpers;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Admin controller.
	/// Only Admin members should be allowed to use it.
	/// </summary>
	public class AdminController : Controller
	{
		/// <summary>
		/// Index this instance.
		/// </summary>
		public ActionResult Index()
		{
			if (!Roles.RoleExists (adminRoleName)) {
				Roles.CreateRole (adminRoleName);
				YavscHelpers.Notify (ViewData, adminRoleName + " " + LocalizedText.role_created);

			}
			return View ();
		}
		/// <summary>
		/// Inits the db.
		/// In order this action succeds, 
		/// there must not exist any administrator,
		/// nor Admin group.
		/// </summary>
		/// <returns>The db.</returns>
		/// <param name="datac">Datac.</param>
		/// <param name="doInit">Do init.</param>
		public ActionResult InitDb(DataAccess datac, string doInit)
		{
			if (doInit=="on") {
				if (ModelState.IsValid) {
					datac.BackupPrefix = Server.MapPath (datac.BackupPrefix);
					DataManager mgr = new DataManager (datac);
					TaskOutput tcdb = mgr.CreateDb ();
					ViewData ["DbName"] = datac.DbName;
					ViewData ["DbUser"] = datac.DbUser;
					ViewData ["Host"] = datac.Host;
					ViewData ["Port"] = datac.Port;
					return View ("Created", tcdb);
				}
			}
			return View ();
		}
		/// <summary>
		/// Backups the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		[Authorize(Roles="Admin")]
		public ActionResult Backups(DataAccess model)
		{
			return View (model);
		}
		/// <summary>
		/// Creates the backup.
		/// </summary>
		/// <returns>The backup.</returns>
		/// <param name="datac">Datac.</param>
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
		/// <summary>
		/// Creates the user backup.
		/// </summary>
		/// <returns>The user backup.</returns>
		/// <param name="datac">Datac.</param>
		/// <param name="username">Username.</param>
		[Authorize(Roles="Admin")]
		public ActionResult CreateUserBackup(DataAccess datac,string username)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Upgrade the specified datac.
		/// </summary>
		/// <param name="datac">Datac.</param>
		[Authorize(Roles="Admin")]
		public ActionResult Upgrade(DataAccess datac) {
			throw new NotImplementedException();
		}
		/// <summary>
		/// Restore the specified datac, backupName and dataOnly.
		/// </summary>
		/// <param name="datac">Datac.</param>
		/// <param name="backupName">Backup name.</param>
		/// <param name="dataOnly">If set to <c>true</c> data only.</param>
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
		/// <summary>
		/// Removes from role.
		/// </summary>
		/// <returns>The from role.</returns>
		/// <param name="username">Username.</param>
		/// <param name="rolename">Rolename.</param>
		/// <param name="returnUrl">Return URL.</param>
		[Authorize(Roles="Admin")]
		public ActionResult RemoveFromRole(string username, string rolename, string returnUrl)
		{
			Roles.RemoveUserFromRole(username,rolename);
			return Redirect(returnUrl);
		}
		/// <summary>
		/// Removes the user.
		/// </summary>
		/// <returns>The user.</returns>
		/// <param name="username">Username.</param>
		/// <param name="submitbutton">Submitbutton.</param>
		[Authorize(Roles="Admin")]
		public ActionResult RemoveUser (string username, string submitbutton)
		{
			ViewData ["usertoremove"] = username;
			if (submitbutton == "Supprimer") {
				Membership.DeleteUser (username);
				YavscHelpers.Notify(ViewData, string.Format("utilisateur \"{0}\" supprimé",username));
				ViewData ["usertoremove"] = null;
			}
			return View ();
		}
		/// <summary>
		/// Removes the role.
		/// </summary>
		/// <returns>The role.</returns>
		/// <param name="rolename">Rolename.</param>
		/// <param name="submitbutton">Submitbutton.</param>
		[Authorize(Roles="Admin")]
		public ActionResult RemoveRole (string rolename, string submitbutton)
		{
			if (submitbutton == "Supprimer") 
			{
				Roles.DeleteRole(rolename);
			}
			return RedirectToAction("RoleList");
		}
		/// <summary>
		/// Removes the role query.
		/// </summary>
		/// <returns>The role query.</returns>
		/// <param name="rolename">Rolename.</param>
		[Authorize(Roles="Admin")]
		public ActionResult RemoveRoleQuery(string rolename)
		{
			ViewData["roletoremove"] = rolename;
			return View ();
		}
		/// <summary>
		/// Removes the user query.
		/// </summary>
		/// <returns>The user query.</returns>
		/// <param name="username">Username.</param>
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
			YavscHelpers.Notify(ViewData, LocalizedText.role_created+ " : "+rolename);
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
			// ASSERT (Roles.RoleExists (adminRoleName)) 
			string [] admins = Roles.GetUsersInRole (adminRoleName);
			string currentUser = Membership.GetUser ().UserName;
			List<SelectListItem> users = new List<SelectListItem> ();
			foreach (MembershipUser u in Membership.GetAllUsers ()) {
				var i = new SelectListItem ();
				i.Text = string.Format ("{0} <{1}>", u.UserName, u.Email);
				i.Value = u.UserName;
				users.Add (i);
			}
			ViewData ["admins"] = admins;
			ViewData ["useritems"] = users;
			if (ModelState.IsValid) {
				Roles.AddUserToRole (model.UserName, adminRoleName);
				YavscHelpers.Notify(ViewData,  model.UserName + " "+LocalizedText.was_added_to_the_role+" '" + adminRoleName + "'");
			} else {
				if (admins.Length > 0) { 
					if (! admins.Contains (Membership.GetUser ().UserName)) {
						ModelState.Remove("UserName");
						ModelState.AddModelError("UserName",LocalizedText.younotadmin+"!");
						return View ("Index");
					}
				} else {
					// No admin, gives the Admin Role to the current user
					Roles.AddUserToRole (currentUser, adminRoleName);
					admins = new string[] { currentUser };
					YavscHelpers.Notify(ViewData, string.Format (
						LocalizedText.was_added_to_the_empty_role,
						currentUser, adminRoleName));
				}
			}
			return View (model);
		}
	}
}

