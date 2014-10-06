using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.RolesAndMembers
{
	public class ChangePasswordModel
	{
		[Required(ErrorMessage = "Please enter a Username")]
		public string Username { get; set; }
 
		[Required(ErrorMessage = "Please your old Password")]
		public string OldPassword { get; set; }
 
		[Required(ErrorMessage = "Please enter a new Password")]
		public string NewPassword { get; set; }

		[Required(ErrorMessage = "Please confirm the new Password")]
		public string ConfirmPassword { get; set; }
 
	}
}

