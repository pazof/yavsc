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
using Yavsc.Model.FrontOffice.Catalog;

namespace Yavsc.ApiControllers
{


	/// <summary>
	/// Front office controller.
	/// </summary>
	public class FrontOfficeController : YavscController
	{

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
			Estimate est = WorkFlowManager.ContentProvider.Get (id);
			string username = Membership.GetUser ().UserName;
			if (est.Client != username)
			if (!Roles.IsUserInRole("Admin"))
			if (!Roles.IsUserInRole("FrontOffice"))
				throw new AuthorizationDenied (
					string.Format (
						"Auth denied to eid {1} for:{2}", 
						id, username));
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
			Estimate e = WorkFlowManager.GetEstimate (estimid);
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
			tmpe.Session.Add ("eto", Membership.GetUser (e.Client).Email);
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
			try {
			new TexToPdfFormatter ().WriteToStream (
				typeof(string), texest, memPdf,null);
			}
			catch (FormatterException ex) {
				return new HttpResponseMessage (HttpStatusCode.OK) { Content = 
					new ObjectContent (typeof(string), ex.Message+"\n\n"+ex.Output+"\n\n"+ex.Error, 
						new ErrorHtmlFormatter (HttpStatusCode.InternalServerError,
							LocalizedText.InternalServerError))
				};
			}

			var result = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new ByteArrayContent(memPdf.GetBuffer())
			};

			result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue ("attachment") {
				FileName = String.Format (
					"Estimation-{0}.pdf",
					id)
			};

			return result;
		}

	}
}

