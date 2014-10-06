using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.RolesAndMembers
{
	public class RegisterViewModel
	{
		[DisplayName("Nom d'utilisateur")]
		[Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur")]
		public string UserName { get; set; }
 
		[DisplayName("Mot de passe")]
		[Required(ErrorMessage = "S'il vous plait, entez un mot de passe")]
		public string Password { get; set; }
 
		[DisplayName("Confirmation du mot de passe")]
		public string ConfirmPassword { get; set; }
 
		[DisplayName("Adresse e-mail")]
		[Required(ErrorMessage = "S'il vous plait, entrez un e-mail valide")]
		public string Email { get; set; }
	}
}

