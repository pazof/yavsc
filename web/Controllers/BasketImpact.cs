using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Http;
using WorkFlowProvider;
using yavscModel.WorkFlow;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Yavsc.ApiControllers
{

	public class BasketImpact 
	{
		public string ProductRef { get; set; }
		public int count { get; set; }
	}

}
