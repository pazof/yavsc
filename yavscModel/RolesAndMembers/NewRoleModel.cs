using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Yavsc.Model.RolesAndMembers
{
	/// <summary>
	/// New role model.
	/// </summary>
	public class NewRoleModel
	{
		/// <summary>
		/// Gets or sets the name of the role.
		/// </summary>
		/// <value>The name of the role.</value>
		[Required]
		[StringLength(255)]
		[DisplayName("Nom du rôle")]
		public string RoleName { get; set; }
	}
}

