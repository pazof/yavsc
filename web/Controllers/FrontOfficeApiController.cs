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

namespace Yavsc.ApiControllers
{

	public class FrontOfficeController : ApiController
	{
		[AcceptVerbs("GET")]
		public Catalog Catalog ()
		{
			return CatalogManager.GetCatalog ();
		}

		[AcceptVerbs("GET")]
		public ProductCategory GetProductCategorie (string brandName, string prodCategorie)
		{
			return CatalogManager.GetCatalog ().GetBrand (brandName).GetProductCategory (prodCategorie)
			;
		}

		[AcceptVerbs("POST")]
		public string Command()
		{
			return null;
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

		[HttpPost]
		public string ProfileImagePost(HttpPostedFile profileImage)
		{
			string[] extensions = { ".jpg", ".jpeg", ".gif", ".bmp", ".png" };
			if (!extensions.Any(x => x.Equals(Path.GetExtension(profileImage.FileName.ToLower()), StringComparison.OrdinalIgnoreCase)))
			{
				throw new HttpResponseException(
					new HttpResponseMessage(HttpStatusCode.BadRequest));
			}

			// string root = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/uploads"); 
			// Other code goes here
			// profileImage.SaveAs ();
			return "/path/to/image.png";
		}

	}
}

