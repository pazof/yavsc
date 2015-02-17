using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// Status change.
	/// </summary>
	public class StatusChange { 
		/// <summary>
		/// Gets or sets the status.
		/// </summary>
		/// <value>The status.</value>
		public int Status { get; set;} 
		/// <summary>
		/// Gets or sets the date.
		/// </summary>
		/// <value>The date.</value>
		public DateTime date { get; set;}
	}
}
