using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// Estimate.
	/// </summary>
	[Serializable]
	public class Estimate : ITitle, IIdentified<long>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.WorkFlow.Estimate"/> class.
		/// </summary>
		public Estimate ()
		{
		}

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		[Required]
		[Display(ResourceType = typeof(LocalizedText),Name="Title")]
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		[Required]
		[DisplayName("Description")]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the responsible.
		/// </summary>
		/// <value>The responsible.</value>
		[Required]
		[DisplayName("Responsable")]
		public string Responsible { get; set; }

		/// <summary>
		/// Gets or sets the client.
		/// </summary>
		/// <value>The client.</value>
		[Required]
		[DisplayName("Client")]
		public string Client { get; set; }

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public long Id { get; set; }


		/// <summary>
		/// Gets the ciffer.
		/// </summary>
		/// <value>The ciffer.</value>
		[Display(ResourceType = typeof(LocalizedText),Name="Ciffer")]
		public decimal Ciffer {
			get {
				decimal total = 0;
				if (Lines == null)
					return total;
				foreach (Writting l in Lines)
					total += l.UnitaryCost * l.Count;
				return total;
			}
		}

		/// <summary>
		/// Gets or sets the lines.
		/// </summary>
		/// <value>The lines.</value>
		public Writting[] Lines { get; set; }
	}
}

