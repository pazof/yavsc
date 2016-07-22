using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc
{

    /// <summary>
    /// Position.
    /// </summary>
    public class Position: IPosition
	{
		/// <summary>
		/// The longitude.
		/// </summary>
        [Required(),Display(Name="Longitude")]
        [Range(-180, 360.0)]

		public double Longitude { get; set; }

		/// <summary>
		///
		/// The latitude.
		/// </summary>
        [Required(),Display(Name="Latitude")]
        [Range(-90, 90 )]
		public double Latitude { get; set; }

	}

    public class Location : Position, ILocation {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required(),
        Display(Name="Address")]
        public string Address { get; set; }

    }
}