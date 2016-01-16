using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Yavsc.Client.FrontOffice
{
	/// <summary>
	/// Commande.
	/// </summary>
	public abstract class Command
	{
		/// <summary>
		/// Gets or sets the creation date.
		/// </summary>
		/// <value>The creation date.</value>
		public DateTime CreationDate { get; set; }

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public long Id { get; set; }

		/// <summary>
		/// Gets or sets the name of the client.
		/// </summary>
		/// <value>The name of the client.</value>
		public string ClientName { get; set; }

		/// <summary>
		/// Gets or sets the product reference.
		/// </summary>
		/// <value>The prod reference.</value>
		public CommandStatus Status { get; set; }

		/// <summary>
		/// Gets or sets the product reference.
		/// </summary>
		/// <value>The product reference.</value>
		public string ProductRef { get; set; }

		/// <summary>
		/// The parameters.
		/// </summary>
		public Dictionary<string,string> Parameters = new Dictionary<string,string> ();

		public abstract string GetDescription ();

	}
}

