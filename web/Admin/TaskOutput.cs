using System;
using System.ComponentModel;

namespace Yavsc.Admin
{
	public class TaskOutput {
		public string Message { get; set; }
		public string Error { get; set; }
		public int ExitCode { get; set; }
	}
	
}
