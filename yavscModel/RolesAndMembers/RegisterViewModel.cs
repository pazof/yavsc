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
		/// Gets or sets the confirm password.
		/// </summary>
		/// <value>The confirm password.</value>
		[DisplayName("Confirmation du mot de passe")]
		public string ConfirmPassword { get; set; }

	}
}

