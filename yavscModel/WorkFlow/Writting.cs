using System;
using System.ComponentModel.DataAnnotations;

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
		[Display(Name="Coût unitaire")]
		[Required(ErrorMessage="Veuillez renseigner un coût unitaire")]
		public decimal UnitaryCost { get; set; }
		/// <summary>
		/// Gets or sets the count.
		/// </summary>
		/// <value>The count.</value>
		[Required(ErrorMessage="Veuillez renseigner un multiplicateur pour cette imputation")]
		public int Count { get; set; }
		/// <summary>
		/// Gets or sets the product reference.
		/// </summary>
		/// <value>The product reference.</value>
		[Required(ErrorMessage="Veuillez renseigner une référence produit")]
		[StringLength(512)]
		public string ProductReference { get; set; }
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		[Required(ErrorMessage="Veuillez renseigner une description de cette imputation.")]
		[StringLength (2048)]
		public string Description { get; set; } 

		public override string ToString ()
		{
			return string.Format ("[Writting: Id={0}, UnitaryCost={1}, Count={2}, ProductReference={3}, Description={4}]", Id, UnitaryCost, Count, ProductReference, Description);
		}
	}
}

