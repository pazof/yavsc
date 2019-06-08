using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace testOauthClient.Controllers
{
    public class HomeController : Controller
    {
        ILogger _logger;
                   
        public class GCMRegistrationRecord {
                     public string   GCMRegistrationId { get; set; } = "testGoogleRegistrationIdValue";
                     public string   DeviceId { get; set; }= "TestDeviceId";
                     public string   Model { get; set; }= "TestModel";
                      public string  Platform { get; set; }= "External Web";
                     public string   Version { get; set; }= "0.0.1-rc1";
                    }
        
        public HomeController(ILoggerFactory loggerFactory)
        {
            _logger=loggerFactory.CreateLogger<HomeController>();
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> GetUserInfo(CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://dev.pschneider.fr/api/me");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);


                var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                return View("Index", model: await response.Content.ReadAsStringAsync());
            }

        }

        [HttpPost]
        public async Task<IActionResult> PostFiles(string subdir)
        {
            string results = null;
            _logger.LogInformation($"{Request.Form.Files.Count} file(s) to send");
            
                // TODO better uri construction in production environment
                    List<FormFile> args = new List<FormFile>();
                    foreach (var formFile in Request.Form.Files)
                    {
                        _logger.LogWarning($"Treating {formFile.ContentDisposition}");
                        var memStream = new MemoryStream();
                        const int sz = 1024*64;
                        byte [] buffer = new byte[sz];
                        using (var innerStream = formFile.OpenReadStream()) {
                            int szRead = 0;
                            do {
                                szRead = innerStream.Read(buffer,0,sz);
                                memStream.Write(buffer,0,szRead);
                            } while (szRead>0);
                        }
                        memStream.Seek(0,SeekOrigin.Begin);
                        args.Add(
                        new FormFile {
                            ContentDisposition = formFile.ContentDisposition,
                            ContentType = formFile.ContentType,
                            Stream = memStream
                        });
                    }
                    string uri = "http://dev.pschneider.fr/api/fs/"+System.Uri.EscapeDataString(subdir);
                    _logger.LogInformation($"Posting data to '{uri}'...");
                    
                    results = await RequestHelper.PostMultipart(uri, args.ToArray(), AccessToken);
                    _logger.LogInformation("Data posted.");
            
            return View("Index", model: results);

        }

        [HttpPost]
        public async Task<IActionResult> PostDeviceInfo(CancellationToken cancellationToken)
        {
            /* 
                   using (var client = new HttpClient()) {
                   var request = new HttpRequestMessage(HttpMethod.Post, "http://dev.pschneider.fr/api/gcm/register");

                   request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                   var json = JsonConvert.
                   SerializeObject(new Yavsc.Models.Identity.GoogleCloudMobileDeclaration  { DeviceId= "devid01", GCMRegistrationId = "1234" } );
                   var content = new StringContent(json, Encoding.UTF8, "application/json");
                   var response = await client.SendAsync(request, cancellationToken);
                   response.EnsureSuccessStatusCode();

                   return View("Index", model: await response.Content.ReadAsStringAsync());
               }*/
            GCMRegistrationRecord result = null;
            var authHeader = $"Bearer {AccessToken}";
            _logger.LogWarning($"using authorization Header {authHeader}");
            try {

            
                using (var request = new SimpleJsonPostMethod(
                    "http://dev.pschneider.fr/api/gcm/register", authHeader))
                {
                    result = await request.Invoke<GCMRegistrationRecord>(new
                   GCMRegistrationRecord {
                        GCMRegistrationId = "testGoogleRegistrationIdValue",
                        DeviceId = "TestDeviceId",
                        Model = "TestModel",
                        Platform = "External Web",
                        Version = "0.0.1-rc1"
                    });
                }
            }
            catch (Exception ex) {
                 return View("Index", model: new { error = ex.Message });
            }
            return View("Index", model: result?.ToString());
        }

        protected string AccessToken
        {
            get
            {
                var claim = HttpContext.User?.FindFirst("access_token");
                if (claim == null)
                {
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
