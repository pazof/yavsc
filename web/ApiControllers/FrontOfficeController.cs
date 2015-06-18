using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Profile;
using System.Web.Security;
using Yavsc.Formatters;
using Yavsc.Helpers;
using Yavsc.Model;
using Yavsc.Model.FrontOffice;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Model.WorkFlow;
using System.IO;

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
		[AcceptVerbs ("GET")]
		public Catalog Catalog ()
		{
			Catalog c = CatalogManager.GetCatalog ();
			return c;
		}

		/// <summary>
		/// Gets the product categorie.
		/// </summary>
		/// <returns>The product categorie.</returns>
		/// <param name="brandName">Brand name.</param>
		/// <param name="prodCategorie">Prod categorie.</param>
		[AcceptVerbs ("GET")]
		public ProductCategory GetProductCategorie (string brandName, string prodCategorie)
		{
			return CatalogManager.GetCatalog ().GetBrand (brandName).GetProductCategory (prodCategorie);
		}

		/// <summary>
		/// Gets the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="id">Estimate Id.</param>
		[Authorize]
		[HttpGet]
		public Estimate GetEstimate (long id)
		{
			Estimate est = wfmgr.ContentProvider.GetEstimate (id);
			return est;
		}

		/// <summary>
		/// Gets the estim tex.
		/// </summary>
		/// <returns>The estim tex.</returns>
		/// <param name="id">Estimate id.</param>
		[AcceptVerbs ("GET")]
		public HttpResponseMessage EstimateToTex (long id)
		{
			string texest = estimateToTex (id);
			if (texest == null)
				throw new InvalidOperationException (
					"Not an estimate");
			HttpResponseMessage result =  new HttpResponseMessage () {
				Content = new ObjectContent (typeof(string),
					texest,
					new SimpleFormatter ("text/x-tex"))
			};
			result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue ("attachment") {
				FileName = "estimate-" + id.ToString () + ".tex" 
			};
			return result;
		}

		private string estimateToTex (long estimid)
		{
			Yavsc.templates.Estim tmpe = new Yavsc.templates.Estim ();		
			Estimate e = wfmgr.GetEstimate (estimid);
			tmpe.Session = new Dictionary<string,object> ();
			tmpe.Session.Add ("estim", e);
			Profile prpro = new Profile (ProfileBase.Create (e.Responsible));
			if (!prpro.HasBankAccount)
				throw new TemplateException ("NotBankable:" + e.Responsible);
			if (!prpro.HasPostalAddress)
				throw new TemplateException ("NoPostalAddress:" + e.Responsible);
			
			Profile prcli = new Profile (ProfileBase.Create (e.Client));
			if (!prcli.IsBillable)
				throw new TemplateException ("NotBillable:" + e.Client);


			tmpe.Session.Add ("from", prpro);
			tmpe.Session.Add ("to", prcli);
			tmpe.Session.Add ("efrom", Membership.GetUser (e.Responsible).Email);
			tmpe.Session.Add ("efrom", Membership.GetUser (e.Client).Email);
			tmpe.Init ();
			return tmpe.TransformText ();
		}

		/// <summary>
		/// Gets the estimate in pdf format from tex generation.
		/// </summary>
		/// <returns>The to pdf.</returns>
		/// <param name="id">Estimid.</param>
		[AcceptVerbs("GET")]
		public HttpResponseMessage EstimateToPdf (long id)
		{
			string texest = null;
			try {
				texest = estimateToTex (id);
			} catch (TemplateException ex) {
				return new HttpResponseMessage (HttpStatusCode.OK) { Content = 
					new ObjectContent (typeof(string),
						ex.Message, new ErrorHtmlFormatter (HttpStatusCode.NotAcceptable,
						LocalizedText.DocTemplateException 
					))
				};
			} catch (Exception ex) {
				return new HttpResponseMessage (HttpStatusCode.OK) { Content = 
					new ObjectContent (typeof(string),
						ex.Message, new ErrorHtmlFormatter (HttpStatusCode.InternalServerError,
						LocalizedText.DocTemplateException))
				};
			}
			if (texest == null)
				return new HttpResponseMessage (HttpStatusCode.OK) { Content = 
					new ObjectContent (typeof(string), "Not an estimation id:" + id, 
						new ErrorHtmlFormatter (HttpStatusCode.NotFound,
							LocalizedText.Estimate_not_found))
				};

			var memPdf = new MemoryStream ();
			HttpResponseMessage result = new HttpResponseMessage ();
			new TexToPdfFormatter ().WriteToStream (
				typeof(string), texest, memPdf,null);
			memPdf.Position = 0;
			var sr = new StreamReader(memPdf);
			var str = sr.ReadToEnd();
			result.Content = new StringContent (str);
			TexToPdfFormatter.SetFileName (result.Content.Headers, "estimate-" + id.ToString ());
			result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/x-tex");
			return result;
		}

	}
}

