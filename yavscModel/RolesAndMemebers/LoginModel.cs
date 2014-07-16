using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace yavscModel.RolesAndMembers
{
	public class LoginModel
	{
		[DisplayName("Nom d'utilisateur")]
		[Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur ([a-z]|[A-Z]|[-_.~])+")]
		[RegularExpression("([a-z]|[A-Z]|[-_.~])+")]
		public string UserName { get; set; }
 
		[DisplayName("Mot de passe")]
		[Required(ErrorMessage = "S'il vous plait, entez un mot de passe")]
		[RegularExpression("([a-z]|[A-Z]|[-_.~#{}`'\\^])+")]
		public string Password { get; set; }

		[Display(Name = "Se souvenir du mot de passe")]
		public bool RememberMe { get; set; }
	}
}
