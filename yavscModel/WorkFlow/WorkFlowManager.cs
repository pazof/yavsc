using System;
using Yavsc.Model.WorkFlow;
using System.Configuration;
using System.Collections.Specialized;
using Yavsc.Model.FrontOffice;
using System.Configuration.Provider;
using Yavsc.Model.FrontOffice.Catalog;
using System.Collections.Generic;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// Work flow manager.
	/// It takes orders store them and raise some events for modules
	/// It publishes estimates and invoices
	/// </summary>
	public static class WorkFlowManager 
	{
		/// <summary>
		/// Finds the activity.
		/// </summary>
		/// <returns>The activity.</returns>
		/// <param name="pattern">Pattern.</param>
		public static IEnumerable<Activity> FindActivity(string pattern = "%")
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the activity.
		/// </summary>
		/// <returns>The activity.</returns>
		/// <param name="MAECode">MAE code.</param>
		public static Activity GetActivity (string MAECode)
		{
			throw new NotImplementedException ();
		}

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
		public static long RegisterCommand(Command com)
		{
			return ContentProvider.RegisterCommand (com);
		}

		/// <summary>
		/// Updates the estimate.
		/// </summary>
		/// <param name="estim">Estim.</param>
		public static void UpdateEstimate (Estimate estim)
		{
			ContentProvider.Update (estim);
		}
		/// <summary>
		/// Gets the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="estid">Estid.</param>
		public static Estimate GetEstimate (long estid)
		{
			return ContentProvider.Get (estid);
		}
		/// <summary>
		/// Gets the estimates, refering the 
		/// given client or username .
		/// </summary>
		/// <returns>The estimates.</returns>
		/// <param name="responsible">Responsible.</param>
		public static Estimate [] GetResponsibleEstimates (string responsible)
		{
			return ContentProvider.GetEstimates (null, responsible);
		}

		/// <summary>
		/// Gets the client estimates.
		/// </summary>
		/// <returns>The client estimates.</returns>
		/// <param name="client">Client.</param>
		public static Estimate [] GetClientEstimates (string client)
		{
			return ContentProvider.GetEstimates (client, null);
		}

		/// <summary>
		/// Gets the user estimates.
		/// </summary>
		/// <returns>The user estimates.</returns>
		/// <param name="username">Username.</param>
		public static Estimate [] GetUserEstimates (string username)
		{
			return ContentProvider.GetEstimates (username);
		}

		/// <summary>
		/// Gets the stock for a given product reference.
		/// </summary>
		/// <returns>The stock status.</returns>
		/// <param name="productReference">Product reference.</param>
		public static StockStatus GetStock(string productReference)
		{
			return ContentProvider.GetStockStatus (productReference);
		}

		/// <summary>
		/// Updates the writting.
		/// </summary>
		/// <param name="wr">Wr.</param>
		public static void UpdateWritting (Writting wr)
		{
			ContentProvider.UpdateWritting (wr);
		}

		/// <summary>
		/// Drops the writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		public static void DropWritting (long wrid)
		{
			ContentProvider.DropWritting (wrid);
		}
		/// <summary>
		/// Drops the estimate.
		/// </summary>
		/// <param name="estid">Estid.</param>
		public static void DropEstimate (long estid)
		{
			ContentProvider.DropEstimate(estid);
		}



		static IContentProvider contentProvider;
		/// <summary>
		/// Gets the content provider.
		/// </summary>
		/// <value>The content provider.</value>
		public static IContentProvider ContentProvider {
			
			get {
				if (contentProvider == null)
					contentProvider = ManagerHelper.GetDefaultProvider
						("system.web/workflow") as IContentProvider; 
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
		public static Estimate CreateEstimate(string responsible, string client, string title, string description)
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
		public static long Write(long estid, string desc, decimal ucost, int count, string productid)
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
		public static void SetEstimateStatus(long estid, int status, string username)
		{
			ContentProvider.SetEstimateStatus (estid, status, username);
		}
		/// <summary>
		/// Gets the commands.
		/// </summary>
		/// <returns>The commands.</returns>
		/// <param name="username">Username.</param>
		public static CommandSet GetCommands(string username)
		{
			return ContentProvider.GetCommands (username);
		}

		public static string [] APEDisponibles
		{
			get {
				return new string[]{ "Chanteur", "DJ", "Musicien", "Clown" };
			}
		}
	}
}

