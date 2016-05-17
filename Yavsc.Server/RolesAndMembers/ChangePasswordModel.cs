using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.RolesAndMembers
{
	/// <summary>
	/// Change password model.
	/// </summary>
	public class ChangePasswordModel
	{
		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		/// <value>The username.</value>
		[Required(ErrorMessage = "Please, enter your user name"),RegularExpression("([a-z]|[A-Z]|[\\s-_.~]|[0-9])+")] 
		public string Username { get; set; }

		/// <summary>
		/// Gets or sets the old password.
		/// </summary>
		/// <value>The old password.</value>
		[Required(ErrorMessage = "Please, enter your old Password")]
		public string OldPassword { get; set; }
 
		/// <summary>
		/// Gets or sets the new password.
		/// </summary>
		/// <value>The new password.</value>
		[Required(ErrorMessage = "Please enter a new Password")]
		public string NewPassword { get; set; }

		/// <summary>
		/// Gets or sets the confirm password.
		/// </summary>
		/// <value>The confirm password.</value>
		[Required(ErrorMessage = "Please confirm the new Password")]
		public string ConfirmPassword { get; set; }
 
	}
}

