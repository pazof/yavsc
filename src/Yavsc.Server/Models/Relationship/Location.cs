using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Relationship
{

    /// <summary>
    /// Position.
    /// </summary>
    public class Position: IPosition
	{
		/// <summary>
		/// The longitude.
		/// </summary>
        [YaRequired(),Display(Name="Longitude")]
        [Range(-180, 360.0)]

		public double Longitude { get; set; }

		/// <summary>
		///
		/// The latitude.
		/// </summary>
        [YaRequired(),Display(Name="Latitude")]
        [Range(-90, 90 )]
		public double Latitude { get; set; }

	}

    public class Location : Position, ILocation {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [YaRequired(),
        Display(Name="Address"),
        MaxLength(512)]
        public string Address { get; set; }
    }
}