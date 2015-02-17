using System;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Period.
	/// </summary>
	public class Period
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Period"/> class.
		/// </summary>
		public Period ()
		{
		}
		/// <summary>
		/// Gets or sets the start date.
		/// </summary>
		/// <value>The start date.</value>
		public DateTime StartDate { get; set; }
		/// <summary>
		/// Gets or sets the end date.
		/// </summary>
		/// <value>The end date.</value>
		public DateTime EndDate { get; set; }

	}
}

