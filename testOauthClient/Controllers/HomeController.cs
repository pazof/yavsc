using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Yavsc.Helpers;

namespace testOauthClient.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetUserInfo(CancellationToken cancellationToken)
        {
            using (var client = new HttpClient()) {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://dev.pschneider.fr/api/me");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                

                var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                return View("Index", model: await response.Content.ReadAsStringAsync());
            }

        }
         
        [HttpPost]
        public async Task<IActionResult> PostDeviceInfo(CancellationToken cancellationToken)
        {
        /*    using (var client = new HttpClient()) {
                var request = new HttpRequestMessage(HttpMethod.Post, "http://dev.pschneider.fr/api/gcm/register");
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                var json = JsonConvert.
                SerializeObject(new Yavsc.Models.Identity.GoogleCloudMobileDeclaration  { DeviceId= "devid01", GCMRegistrationId = "1234" } );
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                return View("Index", model: await response.Content.ReadAsStringAsync());
            }*/
            var res =  await new SimpleJsonPostMethod(
                "http://dev.pschneider.fr/api/gcm/register",
                "Authorization: Bearer "+AccessToken).InvokeJson( new  {
                    GCMRegistrationId = "testGoogleRegistrationIdValue",
              DeviceId = "TestDeviceId",
              Model = "TestModel",
              Platform = "External Web",
              Version = "0.0.1-rc1"
          } );
            return Json(res);
        }
        
        protected string AccessToken {
            get {
                var claim = HttpContext.User?.FindFirst("access_token");
                if (claim == null) {
                    throw new InvalidOperationException("no access_token");
                }

                return claim.Value;
            }
        }
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
