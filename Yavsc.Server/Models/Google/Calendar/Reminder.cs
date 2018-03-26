using System;

namespace Yavsc.Models.Google.Calendar
{
    [Obsolete("use GoogleUse.Apis")]
	public class Reminder {
	/// <summary>
	/// Gets or sets the method.
	/// </summary>
	/// <value>The method.</value>
	public string method { get; set; }
	/// <summary>
	/// Gets or sets the minutes.
	/// </summary>
	/// <value>The minutes.</value>
	public int minutes { get; set; }
}
}