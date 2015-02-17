using System;
using System.ComponentModel.DataAnnotations;
using Yavsc.Model;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// A Writting.
	/// Une ligne d'écriture dans un devis ou une facture
	/// </summary>
	[Serializable]
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
		[Display(ResourceType = typeof(LocalizedText),Name="Unitary_cost")]
		[Required(ErrorMessage="Veuillez renseigner un coût unitaire")]
		public decimal UnitaryCost { get; set; }
		/// <summary>
		/// Gets or sets the count.
		/// </summary>
		/// <value>The count.</value>
		[Display(ResourceType = typeof(LocalizedText),Name="Count")]
		[Required(ErrorMessage="Veuillez renseigner un multiplicateur pour cette imputation")]
		public int Count { get; set; }
		/// <summary>
		/// Gets or sets the product reference.
		/// </summary>
		/// <value>The product reference.</value>
		[Required(ErrorMessage="Veuillez renseigner une référence produit")]
		[StringLength(512)]
		[Display(ResourceType = typeof(LocalizedText),Name="Product_reference")]
		public string ProductReference { get; set; }
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		[Required(ErrorMessage="Veuillez renseigner une description de cette imputation.")]
		[StringLength (2048)]
		[Display(ResourceType = typeof(LocalizedText),Name="Description")]
		public string Description { get; set; } 
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.WorkFlow.Writting"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.WorkFlow.Writting"/>.</returns>
		public override string ToString ()
		{
			return string.Format ("[Writting: Id={0}, UnitaryCost={1}, Count={2}, ProductReference={3}, Description={4}]", Id, UnitaryCost, Count, ProductReference, Description);
		}
	}
}

