using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.RolesAndMembers
{
	/// <summary>
	/// Register view model.
	/// </summary>
	public class RegisterViewModel
	{
		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		[Localizable(true)]
		[Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur")]
		public string UserName { get; set; }
 
		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>The password.</value>
		[DisplayName("Mot de passe")]
		[Required(ErrorMessage = "S'il vous plait, entez un mot de passe")]
		public string Password { get; set; }
 
		/// <summary>
		/// Gets or sets the confirm password.
		/// </summary>
		/// <value>The confirm password.</value>
		[DisplayName("Confirmation du mot de passe")]
		public string ConfirmPassword { get; set; }
 
		/// <summary>
		/// Gets or sets the email.
		/// </summary>
		/// <value>The email.</value>
		[DisplayName("Adresse e-mail")]
		[Required(ErrorMessage = "S'il vous plait, entrez un e-mail valide")]
		public string Email { get; set; }
	}
}

