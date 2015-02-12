using System;
using Yavsc;
using System.Collections.Specialized;
using Yavsc.Model.WorkFlow;
using Newtonsoft.Json;


namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Commande.
	/// </summary>
	public class Commande
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
		/// Gets or sets the prod reference.
		/// </summary>
		/// <value>The prod reference.</value>
		public string ProdRef { get; set; }
		/// <summary>
		/// The parameters.
		/// </summary>
		public StringDictionary Parameters = new StringDictionary();
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Commande"/> class.
		/// </summary>
		public Commande() {
		}
		/// <summary>
		/// Create the specified collection.
		/// </summary>
		/// <param name="collection">Collection.</param>
		public static Commande Create(NameValueCollection collection)
		{
			Commande cmd = new Commande ();
			// string catref=collection["catref"]; // Catalog Url from which formdata has been built
			cmd.ProdRef=collection["ref"]; // Required product reference
			cmd.CreationDate = DateTime.Now;

			// stores the parameters:
			foreach (string key in collection.AllKeys) {
				cmd.Parameters.Add (key, collection [key]);
			}
			WorkFlowManager wm = new WorkFlowManager ();
			wm.RegisterCommand (cmd); // sets cmd.Id
			return cmd;
		}
	}
}

