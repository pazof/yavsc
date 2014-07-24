using System;
using yavscModel.WorkFlow;
using System.Web.Mvc;

namespace yavscModel.WorkFlow
{
	public interface IWFModule
	{
		/// <summary>
		/// Gets the state for an order (assuming it was passed to <c>Handle</c>).
		/// </summary>
		/// <returns>The state.</returns>
		/// <param name="c">C.</param>
		int GetState (IWFOrder c);
		/// <summary>
		/// Handle the specified order and form input value collection.
		/// </summary>
		/// <param name="order">l'ordre</param>
		/// <param name="collection">La collection de valeur de champs d'entée de formulaires.</param>
		/// <returns>0 when the module accepts to handle the order, non null value otherwize<returns>
		int Handle (IWFOrder order,FormCollection collection);

	}
}

