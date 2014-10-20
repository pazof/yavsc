using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// A Writting.
	/// Une ligne d'écriture dans un devis ou une facture
	/// </summary>
	public class Writting
	{
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public long Id { get; set; }
		/// <summary>
		/// Gets or sets the unitary cost, per unit, or per hour ...
 		/// Who knows?
		/// </summary>
		/// <value>The unitary cost.</value>
		[Required()]
		public decimal UnitaryCost { get; set; }
		/// <summary>
		/// Gets or sets the count.
		/// </summary>
		/// <value>The count.</value>
		[Required()]
		public int Count { get; set; }
		/// <summary>
		/// Gets or sets the product reference.
		/// </summary>
		/// <value>The product reference.</value>
		[Required()]
		public string ProductReference { get; set; }
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		[Required()]
		public string Description { get; set; } 
	}
}

