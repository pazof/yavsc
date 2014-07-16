using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace yavscModel.WorkFlow
{
	public class NewProjectModel
	{
		[DisplayName("Nom du projet")] 
		[Required()]
		public string Name { get; set; }

		[DisplayName("Manager du projet")]
		[Required]
		public string Manager { get; set; }

		[DisplayName("Description du projet")]
		[Required]
		public string Description { get; set; }

	}
}

