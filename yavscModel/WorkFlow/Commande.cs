using System;
using Yavsc;
using SalesCatalog;
using SalesCatalog.Model;
using System.Collections.Specialized;


namespace Yavsc.Model.WorkFlow
{
	public class Commande
	{
		public DateTime CreationDate { get; set; }
		long Id { get; set; }
		public Commande(long catid, long pref, NameValueCollection collection)
		{
			CreationDate = DateTime.Now;
			//TODO save it and get the id
		}
	}
}

