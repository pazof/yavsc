using System;
using System.ComponentModel.DataAnnotations;

namespace SalesCatalog.Model
{
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
	
		public abstract string Type { get; }

		private string name=null;

		[StringLength(256)]
		public string Name { get { return name == null ? Id : name; } set { name = value; } }

	}
}

