using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace testOauthClient.Controllers
{
    public class HomeController : Controller
    {
        readonly ILogger _logger;

        public class GCMRegistrationRecord
        {
            public string GCMRegistrationId { get; set; } = "testGoogleRegistrationIdValue";
            public string DeviceId { get; set; } = "TestDeviceId";
            public string Model { get; set; } = "TestModel";
            public string Platform { get; set; } = "External Web";
            public string Version { get; set; } = "0.0.1-rc1";
        }

        public HomeController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HomeController>();
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

    public async Task<IActionResult> CallApi()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var content = await client.GetStringAsync("https://localhost:5001/api/me");

        ViewBag.Json = content;
        return View("json");
    }

        [HttpPost]
        public async Task<IActionResult> GetUserInfo(CancellationToken cancellationToken)
        {
             var accessToken = await HttpContext.GetTokenAsync("access_token");

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5001/api/me");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


                var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                return View("UserInfo", model: await response.Content.ReadAsStringAsync());
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

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
