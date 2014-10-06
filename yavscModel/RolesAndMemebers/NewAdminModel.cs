using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.RolesAndMembers
{
	public class NewAdminModel
	{
		[Required(ErrorMessage = "S'il vous plait, entrez un nom d'utilisateur")]
		[Display(Name = "Nom du nouvel administrateur", Description="Nom de l'utilisateur à enregistrer comme administrateur")]
		public string UserName { get; set ; }
	}
}

