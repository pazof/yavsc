using System;
using Yavsc.Model.WorkFlow;
using System.Configuration;
using Yavsc.Model.WorkFlow.Configuration;
using System.Collections.Specialized;
using SalesCatalog.Model;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// Work flow manager.
	/// It takes orders store them and raise some events for modules
	/// It publishes estimates and invoices
	/// </summary>
	public class WorkFlowManager 
	{
		public static Catalog Catalog { get; set; }

		public void UpdateEstimate (Estimate estim)
		{
			ContentProvider.UpdateEstimate (estim);
		}

		public Estimate GetEstimate (long estid)
		{
			return ContentProvider.GetEstimate (estid);
		}

		public Estimate [] GetEstimates (string client)
		{
			return ContentProvider.GetEstimates (client);
		}

		public void UpdateWritting (Writting wr)
		{
			ContentProvider.UpdateWritting (wr);
		}

		public void DropWritting (long wrid)
		{
			ContentProvider.DropWritting (wrid);
		}
		public void DropEstimate (long estid)
		{
			ContentProvider.DropEstimate(estid);
		}
		IContentProvider contentProvider;

		public IContentProvider ContentProvider {
			get {
				WorkflowConfiguration c = (WorkflowConfiguration) ConfigurationManager.GetSection ("system.web/workflow");
				if (c == null)
					throw new Exception ("No system.web/workflow configuration section found");
				WFProvider confprov = c.Providers.GetElement (c.DefaultProvider);
				if (confprov == null)
					throw new Exception ("Default workflow provider not found (system.web/workflow@defaultProvider)");
				string clsName = confprov.Type;
				if (clsName == null)
					throw new Exception ("Provider type not specified (system.web/workflow@type)");

				if (contentProvider != null)
				{ 
					if (contentProvider.GetType ().Name != clsName) 
						contentProvider = null;
				}
					
				if (contentProvider == null)
				{
					Type cpt = Type.GetType (clsName);
					if (cpt == null)
						throw new Exception (string.Format("Type not found : {0} (wrong name, or missing assembly reference?)",clsName));
					System.Reflection.ConstructorInfo ci =cpt.GetConstructor (System.Type.EmptyTypes);
					contentProvider = (IContentProvider)ci.Invoke (System.Type.EmptyTypes);
				}

				NameValueCollection config = new NameValueCollection ();
				config.Add ("name", confprov.Name);
				config.Add ("connectionStringName", confprov.ConnectionStringName);
				config.Add ("applicationName", confprov.ApplicationName);
				contentProvider.Initialize (confprov.Name, config);

				return contentProvider;


			}
		}

		/// <summary>
		/// Creates the estimate.
		/// </summary>
		/// <returns>The estimate identifier.</returns>
		/// <param name="title">Title.</param>

		public Estimate CreateEstimate(string responsible, string client, string title, string description)
		{
			Estimate created = ContentProvider.CreateEstimate (responsible, client, title, description);
			return created;
		}

		public long Write(long estid, string desc, decimal ucost, int count, string productid)
		{
			if (!string.IsNullOrWhiteSpace(productid)) {
				if (Catalog == null)
					Catalog = SalesCatalog.CatalogManager.GetCatalog ();
				if (Catalog == null)
					throw new Exception ("No catalog");
				Product p = Catalog.FindProduct (productid);
				if (p == null)
					throw new Exception ("Product not found");
				// TODO new EstimateChange Event
			}
			return ContentProvider.Write(estid, desc, ucost, count, productid);
		}

		public void SetEstimateStatus(long estid, int status, string username)
		{
			ContentProvider.SetEstimateStatus (estid, status, username);
		}

	}
}

