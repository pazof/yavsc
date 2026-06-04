using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yavsc.WebApiClient.Helpers;

namespace sampleWebAsWebApiClient.Controllers
{
    public class HomeController(ILoggerFactory loggerFactory,
        OpenIdConnectOptions options

    ) : Controller
    {
        readonly ILogger _logger = loggerFactory.CreateLogger<HomeController>();
        readonly OpenIdConnectOptions connectOptions = options;

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
            var content = await client.GetStringAsync(connectOptions.Authority);

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
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public IActionResult Logout()
        {
            return SignOut(Constants.INTERNAL_SCHEME, Constants.EXTERNAL_SCHEME);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
