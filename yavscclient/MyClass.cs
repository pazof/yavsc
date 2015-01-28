using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Yavsc.Model.FrontOffice;

namespace Yavsc
{
	public class MainClass
	{
		public static string ServiceUrl{ get; set; }

		public static void Main(string [] args)
		{
			foreach (string s in args) {
				if (Uri.IsWellFormedUriString (s,UriKind.Absolute)) {
					// TODO create a client
				}
				GetCatalog ();
			}
		}
		static HttpClient GetClient()
		{
			HttpClient client = new HttpClient ();

			client.BaseAddress = new Uri (ServiceUrl);

			// Add an Accept header for JSON format.
			client.DefaultRequestHeaders.Accept.Add (
				new MediaTypeWithQualityHeaderValue ("application/json"));
			return client;
		}

		static void GetCatalog() {
			HttpClient client = GetClient ();

			HttpResponseMessage response = client.GetAsync("api/FrontOffice/Catalog").Result;  // Blocking call!
			if (response.IsSuccessStatusCode)
			{

				var jsonFormatter = new JsonMediaTypeFormatter();
				jsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
				jsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
				// Parse the response body. Blocking!
				Catalog cat = response.Content.ReadAsAsync<Catalog>(new List<MediaTypeFormatter>{jsonFormatter},null).Result;
				foreach (var p in cat.Brands)
				{
					Console.WriteLine("{0}\t{1};\t{2}", p.Name, p.Categories.Length, p.Slogan);
				}
			}
			else
			{
				Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
			}
		}

		public void UpLoad(string fileName)
		{
			using (var client = GetClient())
			{
				using (var content = new MultipartFormDataContent())
				{
					var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(fileName));//();
					fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
					{
						FileName = fileName
					};
					content.Add(fileContent);
					var result = client.PostAsync(ServiceUrl, content).Result;
				}
			}
		}
	}
}

