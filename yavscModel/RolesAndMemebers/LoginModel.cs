using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Yavsc.Model.RolesAndMembers
{
	/// <summary>
	/// Login model.
	/// </summary>
	public class LoginModel
	{
		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		[DisplayName("Nom d'utilisateur"),
		Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur ([a-z]|[A-Z]|[-_.~])+"),
		RegularExpression("([a-z]|[A-Z]|[-_.~])+")]
		public string UserName { get; set; }
 
		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>The password.</value>
		[DisplayName("Mot de passe"),
		Required(ErrorMessage = "S'il vous plait, entez un mot de passe"),
		RegularExpression("([a-z]|[A-Z]|[-_.~#{}`'\\^])+")]
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.RolesAndMembers.LoginModel"/> remember me.
		/// </summary>
		/// <value><c>true</c> if remember me; otherwise, <c>false</c>.</value>
		[Display(Name = "Remember_me",ResourceType=typeof(LocalizedText))]
		public bool RememberMe { get; set; }
	}
}
