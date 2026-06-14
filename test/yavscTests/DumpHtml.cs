using System.Net.Http;

namespace Yavsc.Tests
{
    [Collection("Yavsc Server")]
    public class DumpHtml : IClassFixture<WebServerFixture>
    {
        readonly WebServerFixture _server;
        public DumpHtml(WebServerFixture server) { _server = server; }

        [Fact]
        [Trait("debug", "html")]
        public void DumpHomePageHtml()
        {
            var settings = _server.SiteSettings;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            };
            var httpsUrl = _server.Addresses.FirstOrDefault(u => u.StartsWith("https:"))
                ?? _server.Addresses.FirstOrDefault() ?? "";
            using var client = new HttpClient(handler) { BaseAddress = new Uri(httpsUrl) };

            var paths = new[] { "/Home/About", "/css/site.css", "/lib/bootstrap.quartz.min.css", "/nonexistent" };
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"BaseAddress = {httpsUrl}");
            foreach (var p in paths)
            {
                var r = client.GetAsync(p).GetAwaiter().GetResult();
                var b = r.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                sb.AppendLine($"GET {p} => {r.StatusCode} len={b.Length} ct={r.Content.Headers.ContentType}");
                if (b.Length > 0 && b.Length < 500)
                    sb.AppendLine($"  body: {b.Substring(0, Math.Min(300, b.Length))}");
            }

            Assert.Fail(sb.ToString());
        }
    }
}
