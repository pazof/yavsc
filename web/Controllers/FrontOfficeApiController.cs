using System;
using Yavsc;
using SalesCatalog;
using SalesCatalog.Model;
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

namespace Yavsc.ApiControllers
{

	public class FrontOfficeController : ApiController
	{

		protected WorkFlowManager wfmgr = null;

		protected override void Initialize (System.Web.Http.Controllers.HttpControllerContext controllerContext)
		{
			base.Initialize (controllerContext);
			wfmgr = new WorkFlowManager ();
		}

		[AcceptVerbs("GET")]
		public Catalog Catalog ()
		{
			Catalog c = CatalogManager.GetCatalog ();
			return c;
		}

		[AcceptVerbs("GET")]
		public ProductCategory GetProductCategorie (string brandName, string prodCategorie)
		{
			return CatalogManager.GetCatalog ().GetBrand (brandName).GetProductCategory (prodCategorie)
			;
		}
		/*

		public HttpResponseMessage Post()
		{
			HttpResponseMessage result = null;
			var httpRequest = HttpContext.Current.Request;
			if (httpRequest.Files.Count > 0)
			{
				string username = HttpContext.Current.User.Identity.Name;
				int nbf = 0;
				foreach(string file in httpRequest.Files)
				{
					var postedFile = httpRequest.Files[file];
					string filePath = HttpContext.Current.Server.MapPath("~/users/"+username+"/"+ postedFile.FileName);
					postedFile.SaveAs(filePath);
					nbf++; 
				}
				result = Request.CreateResponse <string>(HttpStatusCode.Created,
					string.Format("Received {0} files",nbf));

			}
			else
			{
				result = Request.CreateResponse <string>(HttpStatusCode.BadRequest,"No file received");
			}

			return result;
		}
*/
		/*
		[Authorize]
		public IOrderInfo Order()
		{

		}
		*/
		[Authorize]
		[HttpGet]
		/// <summary>
		/// Gets the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="estid">Estid.</param>
		public Estimate GetEstimate (long Id)
		{
			Estimate est = wfmgr.ContentProvider.GetEstimate (Id);
			return est;
		}

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

