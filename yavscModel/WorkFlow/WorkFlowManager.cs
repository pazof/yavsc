using System;
using Yavsc.Model.WorkFlow;
using System.Configuration;
using Yavsc.Model.WorkFlow.Configuration;
using System.Collections.Specialized;
using Yavsc.Model.FrontOffice;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// Work flow manager.
	/// It takes orders store them and raise some events for modules
	/// It publishes estimates and invoices
	/// </summary>
	public class WorkFlowManager 
	{
		/// <summary>
		/// Gets or sets the catalog.
		/// </summary>
		/// <value>The catalog.</value>
		public static Catalog Catalog { get; set; }

		/// <summary>
		/// Registers the command.
		/// </summary>
		/// <returns>The command.</returns>
		/// <param name="com">COM.</param>
		public long RegisterCommand(Command com)
		{
			return ContentProvider.RegisterCommand (com);
		}

		/// <summary>
		/// Updates the estimate.
		/// </summary>
		/// <param name="estim">Estim.</param>
		public void UpdateEstimate (Estimate estim)
		{
			ContentProvider.UpdateEstimate (estim);
		}
		/// <summary>
		/// Gets the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="estid">Estid.</param>
		public Estimate GetEstimate (long estid)
		{
			return ContentProvider.GetEstimate (estid);
		}
		/// <summary>
		/// Gets the estimates.
		/// </summary>
		/// <returns>The estimates.</returns>
		/// <param name="client">Client.</param>
		public Estimate [] GetEstimates (string client)
		{
			return ContentProvider.GetEstimates (client);
		}

		/// <summary>
		/// Gets the stock for a given product reference.
		/// </summary>
		/// <returns>The stock status.</returns>
		/// <param name="productReference">Product reference.</param>
		public StockStatus GetStock(string productReference)
		{
			return ContentProvider.GetStockStatus (productReference);
		}

		/// <summary>
		/// Updates the writting.
		/// </summary>
		/// <param name="wr">Wr.</param>
		public void UpdateWritting (Writting wr)
		{
			ContentProvider.UpdateWritting (wr);
		}

		/// <summary>
		/// Drops the writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		public void DropWritting (long wrid)
		{
			ContentProvider.DropWritting (wrid);
		}
		/// <summary>
		/// Drops the estimate.
		/// </summary>
		/// <param name="estid">Estid.</param>
		public void DropEstimate (long estid)
		{
			ContentProvider.DropEstimate(estid);
		}



		IContentProvider contentProvider;
		/// <summary>
		/// Gets the content provider.
		/// </summary>
		/// <value>The content provider.</value>
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
		/// <returns>The estimate.</returns>
		/// <param name="responsible">Responsible.</param>
		/// <param name="client">Client.</param>
		/// <param name="title">Title.</param>
		/// <param name="description">Description.</param>
		public Estimate CreateEstimate(string responsible, string client, string title, string description)
		{
			Estimate created = ContentProvider.CreateEstimate (responsible, client, title, description);
			return created;
		}

		/// <summary>
		/// Write the specified estid, desc, ucost, count and productid.
		/// </summary>
		/// <param name="estid">Estid.</param>
		/// <param name="desc">Desc.</param>
		/// <param name="ucost">Ucost.</param>
		/// <param name="count">Count.</param>
		/// <param name="productid">Productid.</param>
		public long Write(long estid, string desc, decimal ucost, int count, string productid)
		{
			if (!string.IsNullOrWhiteSpace(productid)) {
				if (Catalog == null)
					Catalog = CatalogManager.GetCatalog ();
				if (Catalog == null)
					throw new Exception ("No catalog");
				Product p = Catalog.FindProduct (productid);
				if (p == null)
					throw new Exception ("Product not found");
				// TODO new EstimateChange Event
			}
			return ContentProvider.Write(estid, desc, ucost, count, productid);
		}

		/// <summary>
		/// Sets the estimate status.
		/// </summary>
		/// <param name="estid">Estid.</param>
		/// <param name="status">Status.</param>
		/// <param name="username">Username.</param>
		public void SetEstimateStatus(long estid, int status, string username)
		{
			ContentProvider.SetEstimateStatus (estid, status, username);
		}
		/// <summary>
		/// Gets the commands.
		/// </summary>
		/// <returns>The commands.</returns>
		/// <param name="username">Username.</param>
		public CommandSet GetCommands(string username)
		{
			return ContentProvider.GetCommands (username);
		}
	}
}

