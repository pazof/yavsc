using System;
using Yavsc;
using System.Collections.Specialized;
using Yavsc.Model.WorkFlow;
using Newtonsoft.Json;


namespace Yavsc.Model.FrontOffice
{
	public class Commande
	{
		public DateTime CreationDate { get; set; }
		public long Id { get; set; }
		public string ProdRef { get; set; }

		public StringDictionary Parameters = new StringDictionary();

		public Commande() {
		}

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

