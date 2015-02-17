using System;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Stock status.
	/// </summary>
	public enum StockStatus:int
	{
		/// <summary>
		/// not in the catalog.
		/// </summary>
		NonExistent, // 
		/// <summary>
		/// Service resources are not immediatly available.
		/// </summary>
		NotAvailable, // 
		/// <summary>
		/// for a service, the resources are available,
		/// but a human review of the order has to de made by
		/// the FrontOffice, before the process could start.
		/// For a product, it is in the catalog but do not (yet)
		/// leads to any payment, and so, can not be sold.
		/// </summary>
		EstimateRequired,
		/// <summary>
		/// Service is up. For a phisical product, 
		/// it is in stock.
		/// For a service, the resources are available 
		/// and it can be rendered right now
		/// </summary>
		Available, 
		/// <summary>
		/// This service is closed, or this product is no more available in stock.
		/// </summary>
		Spent
	}
}

