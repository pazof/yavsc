using System;
using Yavsc;
using SalesCatalog;
using System.Web.Routing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web.Http;
using System.Net.Http;
using System.Web;
using System.Linq;
using System.IO;
using System.Net;
using WorkFlowProvider;
using System.Web.Security;
using Yavsc.Model.WorkFlow;
using System.Reflection;
using System.Collections.Generic;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Controllers;
using Yavsc.Formatters;
using System.Text;
using System.Web.Profile;
using System.Collections.Specialized;
using Yavsc.Model;
using Yavsc.Model.FrontOffice;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Front office controller.
	/// </summary>
	public class FrontOfficeController : ApiController
	{
		/// <summary>
		/// The wfmgr.
		/// </summary>
		protected WorkFlowManager wfmgr = null;

		/// <summary>
		/// Initialize the specified controllerContext.
		/// </summary>
		/// <param name="controllerContext">Controller context.</param>
		protected override void Initialize (System.Web.Http.Controllers.HttpControllerContext controllerContext)
		{
			base.Initialize (controllerContext);
			wfmgr = new WorkFlowManager ();
		}

		/// <summary>
		/// Catalog this instance.
		/// </summary>
		[AcceptVerbs("GET")]
		public Catalog Catalog ()
		{
			Catalog c = CatalogManager.GetCatalog (Request.RequestUri.AbsolutePath);
			return c;
		}
		/// <summary>
		/// Gets the product categorie.
		/// </summary>
		/// <returns>The product categorie.</returns>
		/// <param name="brandName">Brand name.</param>
		/// <param name="prodCategorie">Prod categorie.</param>
		[AcceptVerbs("GET")]
		public ProductCategory GetProductCategorie (string brandName, string prodCategorie)
		{
			return CatalogManager.GetCatalog (Request.RequestUri.AbsolutePath).GetBrand (brandName).GetProductCategory (prodCategorie)
			;
		}

		/// <summary>
		/// Gets the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="Id">Estimate Id.</param>
		[Authorize]
		[HttpGet]
		public Estimate GetEstimate (long Id)
		{
			Estimate est = wfmgr.ContentProvider.GetEstimate (Id);
			return est;
		}

		/// <summary>
		/// Gets the estim tex.
		/// </summary>
		/// <returns>The estim tex.</returns>
		/// <param name="estimid">Estimid.</param>
		[AcceptVerbs("GET")]
		public HttpResponseMessage GetEstimTex(long estimid)
		{
			string texest = null;
			try {
				texest = getEstimTex (estimid);
			}
			catch (TemplateException ex) {
				return new HttpResponseMessage (HttpStatusCode.OK){ Content = 
					new ObjectContent (typeof(string),
						ex.Message, new ErrorHtmlFormatter(HttpStatusCode.NotAcceptable,
							LocalizedText.DocTemplateException
							))};
			}
			catch (Exception ex) {

				return new HttpResponseMessage (HttpStatusCode.OK){ Content = 
					new ObjectContent (typeof(string),
						ex.Message, new SimpleFormatter("text/text")) };
			}
			if (texest == null)
				return new HttpResponseMessage (HttpStatusCode.OK) { Content = 
					new ObjectContent (typeof(string),
						"Not an estimation id:" + estimid, new SimpleFormatter ("text/text"))
				};

			return new HttpResponseMessage () {
				Content = new ObjectContent (typeof(string),
					texest,
					new SimpleFormatter ("text/x-tex"))
			};
		}

		private string getEstimTex(long estimid)
		{
			Yavsc.templates.Estim  tmpe = new Yavsc.templates.Estim();		
			Estimate e = wfmgr.GetEstimate (estimid);
			tmpe.Session = new Dictionary<string,object>();
			tmpe.Session.Add ("estim", e);

			Profile prpro = new Profile(ProfileBase.Create(e.Responsible));
			if (!prpro.HasBankAccount)
				throw new TemplateException ("NotBankable:"+e.Responsible);

			Profile prcli = new Profile(ProfileBase.Create(e.Client));
			if (!prcli.IsBillable)
				throw new TemplateException ("NotBillable:"+e.Client);
			tmpe.Session.Add ("from", prpro);
			tmpe.Session.Add ("to", prcli);
			tmpe.Init ();
			return tmpe.TransformText ();
		}


		/// <summary>
		/// Gets the estimate in pdf format from tex generation.
		/// </summary>
		/// <returns>The estim pdf.</returns>
		/// <param name="estimid">Estimid.</param>
		public HttpResponseMessage GetEstimPdf(long estimid)
		{
			Estimate estim = wfmgr.GetEstimate (estimid);
			//TODO better with pro.IsBankable && cli.IsBillable

			return new HttpResponseMessage () {
				Content = new ObjectContent (
					typeof(Estimate),
					estim,
					new EstimToPdfFormatter ())
			};
		}
	}
}

