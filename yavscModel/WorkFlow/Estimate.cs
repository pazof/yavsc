using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Yavsc.Model.WorkFlow
{
	[Serializable]
	public class Estimate 
	{
		public Estimate ()
		{
		}
		[Required]
		[DisplayName("Titre")]
		public string Title { get; set; }
		[Required]
		[DisplayName("Description")]
		public string Description { get; set; }
		[Required]
		[DisplayName("Responsable")]
		public string Responsible { get; set; }
		[Required]
		[DisplayName("Client")]
		public string Client { get; set; }

		public long Id { get; set; }
		[DisplayName("Chiffre")]
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

		public Writting[] Lines { get; set; }
	}
}

