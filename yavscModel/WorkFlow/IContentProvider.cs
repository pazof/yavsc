using System;
using System.Collections.Generic;
using Yavsc.Model.FrontOffice.Catalog;
using Yavsc.Model.FrontOffice;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// Interface content provider.
	/// Class Assertion: <c>Statuses.Length &gt;= FinalStatuses.Length</c>.
	/// </summary>
	public interface IContentProvider : IDbModule, IDisposable, IDataProvider<Estimate,long>
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

		string Name { get; }
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
		/// Drops the tag writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		/// <param name="tag">Tag.</param>
		void DropWrittingTag (long wrid,string tag);

		/// <summary>
		/// Finds the activity.
		/// </summary>
		/// <returns>The activity.</returns>
		/// <param name="pattern">Pattern.</param>
		/// <param name="exerted">If set to <c>true</c> exerted.</param>
		Activity [] FindActivity (string pattern, bool exerted);

		/// <summary>
		/// Finds the performer.
		/// </summary>
		/// <returns>The performer.</returns>
		/// <param name="MEACode">MEA code.</param>
		PerformerProfile [] FindPerformer (string MEACode);
		/// <summary>
		/// Gets the activity.
		/// </summary>
		/// <returns>The activity.</returns>
		/// <param name="MEACode">MAE code.</param>
		Activity GetActivity (string MEACode);

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
		StatusChange[] GetEstimateStatuses (long estid);		/// <summary>

		/// <summary>
		/// Gets the estimates.
		/// </summary>
		/// <returns>The estimates.</returns>
		/// <param name="username">Username.</param>
		Estimate [] GetEstimates(string username);

		/// <summary>
		/// Registers the activity.
		/// </summary>
		/// <param name="activity">Activity.</param>
		/// <param name="code">Code.</param>
		void RegisterActivity (string activityName, string meacode, string comment);

		/// <summary>
		/// Gets the estimates.
		/// </summary>
		/// <returns>The estimates.</returns>
		/// <param name="client">Client.</param>
		/// <param name="responsible">Responsible.</param>
		Estimate [] GetEstimates(string client, string responsible);


		/// <summary>
		/// Gets the commands.
		/// </summary>
		/// <returns>The commands.</returns>
		/// <param name="username">Username.</param>
		CommandSet GetCommands (string username);

		/// <summary>
		/// Gets the stock status.
		/// </summary>
		/// <returns>The stock status.</returns>
		/// <param name="productReference">Product reference.</param>
		StockStatus GetStockStatus (string productReference);

		/// <summary>
		/// Registers the command.
		/// </summary>
		/// <returns>The command id in db.</returns>
		/// <param name="com">COM.</param>
		long RegisterCommand (Command com);
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
		/// Tags the writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		/// <param name="tag">Tag.</param>
		void TagWritting (long wrid,string tag);

		/// <summary>
		/// Updates the writting.
		/// </summary>
		/// <param name="wr">Wr.</param>
		void UpdateWritting (Writting wr);

		/// Add a line to the specified estimate by id,
		/// using the specified desc, ucost, count and productid.
		/// </summary>
		/// <param name="estid">Estimate identifier.</param>
		/// <param name="desc">Textual description for this line.</param>
		/// <param name="ucost">Unitary cost.</param>
		/// <param name="count">Cost multiplier.</param>
		/// <param name="productid">Product identifier.</param>
		long Write (long estid, string desc, decimal ucost, int count, string productid);

	}
}

