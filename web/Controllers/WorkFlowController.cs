﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Http;
using WorkFlowProvider;
using yavscModel.WorkFlow;
using System.Web.Http.Controllers;

namespace Yavsc.ApiControllers
{
	[HttpControllerConfiguration(ActionValueBinder=typeof(Basic.MvcActionValueBinder))]
	public class WorkFlowController : ApiController
    {
		[HttpGet]
		[Authorize]
		public object Index()
        {
			
			return new { test="Hello World" }; 
        }
	
		[HttpGet]
		public object Order (BasketImpact bi)
		{
			return new { c="lmk,", message="Panier impacté", impactRef=bi.ProductRef, count=bi.count};
		}

		[HttpGet]
		[Authorize]
		public long Write (long estid, string desc, decimal ucost, int count, long productid=0) {
			// TODO ensure estid owner matches the current one

			return WFManager.Write(estid, desc, ucost, count, productid);
		}

		[Authorize]
		[HttpGet]
		/// <summary>
		/// Gets the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="estid">Estid.</param>
		public Estimate GetEstimate (long estid)
		{
			return WFManager.ContentProvider.GetEstimate (estid);
		}
		/*
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

*/
    }
}
