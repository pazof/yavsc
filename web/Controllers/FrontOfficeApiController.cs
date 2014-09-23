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

namespace Yavsc.ApiControllers
{

	public class FrontOfficeController : ApiController
	{
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

		[AcceptVerbs("GET","POST")]
		public string Command()
		{
			throw new NotImplementedException();
		}

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


		[HttpGet]
		[Authorize]
		public long AddToBasket (string title)
		{
			//TODO find the basket for Membership.GetUser().UserName
			//return WFManager.Write(estid << from the basket, desc, ucost, count, productid);
			throw new NotImplementedException ();
		}
	}
}

