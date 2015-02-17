using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Yavsc.Model.FrontOffice;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// Interface content provider.
	/// Class Assertion: <c>Statuses.Length &gt;= FinalStatuses.Length</c>.
	/// </summary>
	public interface IContentProvider : IModule, IDisposable
	{

		/// <summary>
		/// Gets the different status labels.
		/// 0 is the starting status. Each status is an integer and the 0-based index
		/// of a string in this array.
		/// </summary>
		/// <value>The status labels.</value>
		string [] Statuses { get; }

		/// <summary>
		/// Gets the final statuses.
		/// </summary>
		/// <value>The final statuses.</value>
		bool [] FinalStatuses { get; }
		/// <summary>
		/// Gets the writting status changes.
		/// </summary>
		/// <returns>The writting statuses.</returns>
		/// <param name="wrid">Wrid.</param>
		StatusChange[] GetWrittingStatuses (long wrid);
		/// <summary>
		/// Gets the estimate status changes.
		/// </summary>
		/// <returns>The estimate statuses.</returns>
		/// <param name="estid">Estid.</param>
		StatusChange[] GetEstimateStatuses (long estid);
		/// <summary>
		/// Creates the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="responsible">Responsible.</param>
		/// <param name="client">Client.</param>
		/// <param name="title">Title.</param>
		/// <param name="description">Description.</param>
		Estimate CreateEstimate (string responsible, string client, string title, string description);
		/// <summary>
		/// Add a line to the specified estimate by id,
		/// using the specified desc, ucost, count and productid.
		/// </summary>
		/// <param name="estid">Estimate identifier.</param>
		/// <param name="desc">Textual description for this line.</param>
		/// <param name="ucost">Unitary cost.</param>
		/// <param name="count">Cost multiplier.</param>
		/// <param name="productid">Product identifier.</param>
		long Write (long estid, string desc, decimal ucost, int count, string productid);
		/// <summary>
		/// Gets the estimate by identifier.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="estimid">Estimid.</param>
		Estimate GetEstimate (long estimid);
		/// <summary>
		/// Gets the estimates created for a specified client.
		/// </summary>
		/// <returns>The estimates.</returns>
		/// <param name="client">Client.</param>
		Estimate [] GetEstimates(string client);
		/// <summary>
		/// Drops the writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		void DropWritting (long wrid);
		/// <summary>
		/// Drops the estimate.
		/// </summary>
		/// <param name="estid">Estid.</param>
		void DropEstimate (long estid);
		/// <summary>
		/// Tags the writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		/// <param name="tag">Tag.</param>
		void TagWritting (long wrid,string tag);
		/// <summary>
		/// Drops the tag writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		/// <param name="tag">Tag.</param>
		void DropWrittingTag (long wrid,string tag);
		/// <summary>
		/// Updates the writting.
		/// </summary>
		/// <param name="wr">Wr.</param>
		void UpdateWritting (Writting wr);
		/// <summary>
		/// Updates the estimate.
		/// </summary>
		/// <param name="estim">Estim.</param>
		void UpdateEstimate (Estimate estim);
		/// <summary>
		/// Sets the writting status.
		/// </summary>
		/// <param name="wrtid">Wrtid.</param>
		/// <param name="status">Status.</param>
		/// <param name="username">Username.</param>
		void SetWrittingStatus (long wrtid,int status,string username);
		/// <summary>
		/// Sets the estimate status.
		/// </summary>
		/// <param name="estid">Estid.</param>
		/// <param name="status">Status.</param>
		/// <param name="username">Username.</param>
		void SetEstimateStatus (long estid,int status,string username);
		/// <summary>
		/// Registers the command.
		/// </summary>
		/// <returns>The command id in db.</returns>
		/// <param name="com">COM.</param>
		long RegisterCommand (Commande com);

		/// <summary>
		/// Gets the stock status.
		/// </summary>
		/// <returns>The stock status.</returns>
		/// <param name="productReference">Product reference.</param>
		StockStatus GetStockStatus (string productReference);
	}
}

