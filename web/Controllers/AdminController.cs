using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Security;
using yavscModel.RolesAndMembers;

namespace Yavsc.Controllers.Controllers
{
	public class AdminController : Controller
	{
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
		[Authorize()]
		public ActionResult UserList ()
		{
			MembershipUserCollection c = Membership.GetAllUsers ();
			return View (c);
		}

		[Authorize(Roles="Admin")]
		public ActionResult AddRole ()
		{
			return View ();
		}

		[Authorize(Roles="Admin")]
		public ActionResult DoAddRole (string rolename)
		{
			Roles.CreateRole(rolename);
			ViewData["Message"] = "Rôle créé : "+rolename;
			return View ();
		}

		[Authorize()]
		public ActionResult RoleList ()
		{
			return View (Roles.GetAllRoles ());
		}
		private const string adminRoleName = "Admin";
		protected override void Initialize (System.Web.Routing.RequestContext requestContext)
		{
			base.Initialize (requestContext);
			if (!Roles.RoleExists (adminRoleName)) {
				Roles.CreateRole (adminRoleName);
			}
		}

		[Authorize()]
		public ActionResult Admin (NewAdminModel model)
		{
			string currentUser = Membership.GetUser ().UserName;
			if (ModelState.IsValid) {
				Roles.AddUserToRole (model.UserName, adminRoleName);
				ViewData ["Message"] = model.UserName + " was added to the role '" + adminRoleName + "'";
			} else {
				// assert (Roles.RoleExists (adminRoleName)) 
				string [] admins = Roles.GetUsersInRole (adminRoleName);
				if (admins.Length > 0) { 
					if (! admins.Contains (Membership.GetUser ().UserName)) {
						ModelState.Remove("UserName");
						ModelState.AddModelError("UserName", "You're not administrator!");
						return View ("Index");
					}
				} else {
					Roles.AddUserToRole (currentUser, adminRoleName);
					admins = new string[] { currentUser };
					ViewData ["Message"] += string.Format (
						"There was no user in the 'Admin' role. You ({0}) was just added as the firt user in the 'Admin' role. ", currentUser);
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

