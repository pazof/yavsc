using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.RolesAndMembers
{
	/// <summary>
	/// Register view model.
	/// </summary>
	public class RegisterViewModel : RegisterModel
	{
		/// <summary>
		/// Gets or sets the password confirmation .
		/// </summary>
		/// <value>The confirm password.</value>
		[DisplayName("Confirmation du mot de passe")]
		public string ConfirmPassword { get; set; }

		/// <summary>
		/// Gets or sets the return URL.
		/// </summary>
		/// <value>The return URL.</value>
		public string ReturnUrl { get; set; }
	}
}

