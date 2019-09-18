using System.ComponentModel.DataAnnotations;
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
}