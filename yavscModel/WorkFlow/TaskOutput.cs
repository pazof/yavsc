using System;
using System.ComponentModel;

namespace Yavsc.Admin
{

	/// <summary>
	/// Task output.
	/// </summary>
	public class TaskOutput {
		/// <summary>
		/// Gets or sets the message.
		/// </summary>
		/// <value>The message.</value>
		public string Message { get; set; }
		/// <summary>
		/// Gets or sets the error.
		/// </summary>
		/// <value>The error.</value>
		public string Error { get; set; }
		/// <summary>
		/// Gets or sets the exit code.
		/// </summary>
		/// <value>The exit code.</value>
		public int ExitCode { get; set; }
	}
	
}
