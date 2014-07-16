using System;

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

		public string Id { get; set; }
		private string name=null;
		public string Name { get { return name == null ? Id : name; } set { name = value; } }
	}
}

