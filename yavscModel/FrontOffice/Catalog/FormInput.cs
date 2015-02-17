using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Form input.
	/// </summary>
	public abstract class FormInput: FormElement
	{
		/// <summary>
		/// Gets or sets the identifier, unique in its Form.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[Required]
		[StringLength(256)]
		public string Id { get; set; }
	
		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>The type.</value>
		public abstract string Type { get; }

		private string name=null;
		
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[StringLength(256)]
		public string Name { get { return name == null ? Id : name; } set { name = value; } }

	}
}

