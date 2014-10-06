using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Yavsc.Model.RolesAndMembers
{
	public class NewRoleModel
	{
		[Required]
		[StringLength(255)]
		[DisplayName("Nom du rôle")]
		public string RoleName { get; set; }
	}
}

