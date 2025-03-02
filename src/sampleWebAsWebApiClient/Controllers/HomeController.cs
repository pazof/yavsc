using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Yavsc.WebApiClient.Helpers;

namespace sampleWebAsWebApiClient.Controllers
{
    public class HomeController(ILoggerFactory loggerFactory) : Controller
    {
        readonly ILogger _logger = loggerFactory.CreateLogger<HomeController>();

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
            var content = await client.GetStringAsync("https://localhost:6001/identity");

            ViewBag.Json = content;
            return View("json");
        }
        [HttpPost]
        public async Task<IActionResult> GetUserInfo(CancellationToken cancellationToken)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            string json = await HttpContext.GetJson("https://localhost:6001/api/account/me");
            return View("UserInfo", json);
        }

        [HttpPost]
        public async Task<IActionResult> GetIdentity(CancellationToken cancellationToken)
        {
            string json = await HttpContext.GetJson("https://localhost:6001/identity");
            return View("UserInfo", json);
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
            return SignOut("Cookies", "Yavsc");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
