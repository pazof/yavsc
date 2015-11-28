using System;
using System.Configuration.Provider;
using Yavsc.Model.WorkFlow;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Catalog provider.<br/>
	/// Abstract class, inherited to implement a catalog provider.
	/// </summary>
	public abstract class CatalogProvider: ProviderBase, IContentProvider
	{
		/// <summary>
		/// Creates the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="responsible">Responsible.</param>
		/// <param name="client">Client.</param>
		/// <param name="title">Title.</param>
		/// <param name="description">Description.</param>
		public virtual Estimate CreateEstimate (string responsible, string client, string title, string description)
		{
			return WorkFlowManager.CreateEstimate (responsible, client, title, description);
		}

		/// <summary>
		/// Drops the writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		public virtual void DropWritting (long wrid)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Drops the estimate.
		/// </summary>
		/// <param name="estid">Estid.</param>
		public void DropEstimate (long estid)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Drops the tag writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		/// <param name="tag">Tag.</param>
		public void DropWrittingTag (long wrid, string tag)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Finds the activity.
		/// </summary>
		/// <returns>The activity.</returns>
		/// <param name="pattern">Pattern.</param>
		/// <param name="exerted">If set to <c>true</c> exerted.</param>
		public abstract Activity[] FindActivity (string pattern, bool exerted);

		/// <summary>
		/// Finds the performer.
		/// </summary>
		/// <returns>The performer.</returns>
		/// <param name="MEACode">MEA code.</param>
		public abstract PerformerProfile[] FindPerformer (string MEACode);

		/// <summary>
		/// Gets the activity.
		/// </summary>
		/// <returns>The activity.</returns>
		/// <param name="MEACode">MAE code.</param>
		public abstract Activity GetActivity (string MEACode);

		/// <summary>
		/// Gets the writting status changes.
		/// </summary>
		/// <returns>The writting statuses.</returns>
		/// <param name="wrid">Wrid.</param>
		public StatusChange[] GetWrittingStatuses (long wrid)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the estimate status changes.
		/// </summary>
		/// <returns>The estimate statuses.</returns>
		/// <param name="estid">Estid.</param>
		public StatusChange[] GetEstimateStatuses (long estid)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the estimates.
		/// </summary>
		/// <returns>The estimates.</returns>
		/// <param name="username">Username.</param>
		public Estimate[] GetEstimates (string username)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Registers the activity.
		/// </summary>
		/// <param name="activityName">Activity name.</param>
		/// <param name="meacode">Meacode.</param>
		/// <param name="comment">Comment.</param>
		public abstract void RegisterActivity (string activityName, string meacode, string comment);
		/// <summary>
		/// Gets the estimates.
		/// </summary>
		/// <returns>The estimates.</returns>
		/// <param name="client">Client.</param>
		/// <param name="responsible">Responsible.</param>
		public Estimate[] GetEstimates (string client, string responsible)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the commands.
		/// </summary>
		/// <returns>The commands.</returns>
		/// <param name="username">Username.</param>
		public CommandSet GetCommands (string username)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Gets the stock status.
		/// </summary>
		/// <returns>The stock status.</returns>
		/// <param name="productReference">Product reference.</param>
		public StockStatus GetStockStatus (string productReference)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Registers the command.
		/// </summary>
		/// <returns>The command id in db.</returns>
		/// <param name="com">COM.</param>
		public long RegisterCommand (Command com)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Sets the writting status.
		/// </summary>
		/// <param name="wrtid">Wrtid.</param>
		/// <param name="status">Status.</param>
		/// <param name="username">Username.</param>
		public void SetWrittingStatus (long wrtid, int status, string username)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Sets the estimate status.
		/// </summary>
		/// <param name="estid">Estid.</param>
		/// <param name="status">Status.</param>
		/// <param name="username">Username.</param>
		public void SetEstimateStatus (long estid, int status, string username)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Tags the writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		/// <param name="tag">Tag.</param>
		public void TagWritting (long wrid, string tag)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Updates the writting.
		/// </summary>
		/// <param name="wr">Wr.</param>
		public void UpdateWritting (Writting wr)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Write the specified estid, desc, ucost, count and productid.
		/// </summary>
		/// <param name="estid">Estid.</param>
		/// <param name="desc">Desc.</param>
		/// <param name="ucost">Ucost.</param>
		/// <param name="count">Count.</param>
		/// <param name="productid">Productid.</param>
		public long Write (long estid, string desc, decimal ucost, int count, string productid)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Gets the different status labels.
		/// 0 is the starting status. Each status is an integer and the 0-based index
		/// of a string in this array.
		/// </summary>
		/// <value>The status labels.</value>
		public string[] Statuses {
			get {
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Gets the final statuses.
		/// </summary>
		/// <value>The final statuses.</value>
		public bool[] FinalStatuses {
			get {
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Get the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public Estimate Get (long id)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Update the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		public void Update (Estimate data)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Yavsc.Model.FrontOffice.Catalog.CatalogProvider"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the
		/// <see cref="Yavsc.Model.FrontOffice.Catalog.CatalogProvider"/>. The <see cref="Dispose"/> method leaves the
		/// <see cref="Yavsc.Model.FrontOffice.Catalog.CatalogProvider"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the
		/// <see cref="Yavsc.Model.FrontOffice.Catalog.CatalogProvider"/> so the garbage collector can reclaim the memory that
		/// the <see cref="Yavsc.Model.FrontOffice.Catalog.CatalogProvider"/> was occupying.</remarks>
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Install the model in database using the specified cnx.
		/// </summary>
		/// <param name="cnx">Cnx.</param>
		public void Install (System.Data.IDbConnection cnx)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Uninstall the module data and data model from 
		/// database, using the specified connection.
		/// </summary>
		/// <param name="cnx">Cnx.</param>
		/// <param name="removeConfig">If set to <c>true</c> remove config.</param>
		public void Uninstall (System.Data.IDbConnection cnx, bool removeConfig)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the catalog.
		/// </summary>
		/// <returns>The catalog.</returns>
		public abstract Catalog GetCatalog ();
	}
}

