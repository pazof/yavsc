using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ITContentProvider.Model
{
	/// <summary>
	/// New project model.
	/// </summary>
	public class NewProjectModel
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[DisplayName("Nom du projet")] 
		[Required()]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the manager.
		/// </summary>
		/// <value>The manager.</value>
		[DisplayName("Manager du projet")]
		[Required]
		public string Manager { get; set; }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		[DisplayName("Description du projet")]
		[Required]
		public string Description { get; set; }

	}
}

