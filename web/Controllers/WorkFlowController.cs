using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Http;
using WorkFlowProvider;
using yavscModel.WorkFlow;

namespace Yavsc.ApiControllers
{
	public class WorkFlowController : ApiController
    {
		[HttpGet]
		public object Index()
        {
			return new { test="Hello World" }; 
        }
	

	public object Details(int id)
        {
			throw new NotImplementedException ();
        }

		public object Create()
        {
			throw new NotImplementedException ();
        } 
		public object Edit(int id)
        {
			throw new NotImplementedException ();
        }

		public object Delete(int id)
        {
			throw new NotImplementedException ();
        }

		IContentProvider contentProvider = null;
		IContentProvider ContentProvider {
			get {
				if (contentProvider == null )
					contentProvider = WFManager.GetContentProviderFWC ();
				return contentProvider;
			}
		}


    }
}
