using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.RolesAndMembers
{
	/// <summary>
	/// New admin model.
	/// </summary>
	public class NewAdminModel
	{
		/// <summary>
		/// Gets or sets the name of the user about to become Admin.
		/// </summary>
		/// <value>The name of the user.</value>
		[Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur"),
			Display(Name = "Nom du nouvel administrateur", Description="Nom de l'utilisateur à enregistrer comme administrateur")]
		public string UserName { get; set ; }
	}
}

