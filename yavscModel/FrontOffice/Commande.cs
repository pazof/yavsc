using System;
using Yavsc;
using SalesCatalog;
using SalesCatalog.Model;
using System.Collections.Specialized;
using Yavsc.Model.WorkFlow;


namespace Yavsc.Model.FrontOffice
{
	public class Commande
	{
		public DateTime CreationDate { get; set; }
		public long Id { get; set; }
		public string ProdRef { get; set; }
		public Commande() {
		}

		public static Commande Create(NameValueCollection collection)
		{
			Commande cmd = new Commande ();
			// string catref=collection["catref"]; // Catalog Url from which formdata has been built
			cmd.ProdRef=collection["ref"]; // Required product reference
			cmd.CreationDate = DateTime.Now;
			WorkFlowManager wm = new WorkFlowManager ();
			wm.RegisterCommand (cmd); // sets cmd.Id
			return cmd;
		}
	}
}

