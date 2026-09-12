using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Models;
using Yavsc.Server.Models.IT.SourceCode;
using Microsoft.EntityFrameworkCore;

using Yavsc.Server.Models.IT;

namespace Yavsc.Org.Tests
{
    [Collection("Yavsc Server")]
    [Trait("regression", "oui")]
    public class BaseTestContext : IClassFixture<WebServerFixture>, IDisposable
    {
        public readonly WebServerFixture _serverFixture;
        private readonly ITestOutputHelper _output;

        public BaseTestContext(ITestOutputHelper output, WebServerFixture fixture)
        {
            this._serverFixture = fixture;
            this._output = output;
        }

        public HttpClient CreateHttpClient()
        {
            return new HttpClient(new BypassSslValidationHandler())
            {
                BaseAddress = new Uri(this._serverFixture.HttpsAuthority ?? throw new InvalidOperationException("Missing HttpsAuthority"))
            };
        }

        /// <summary>
        /// Issue a GET against <paramref name="relativePath"/> on the
        /// in-memory test server. Returns the raw HttpResponseMessage
        /// without following redirects — the test asserts on the first
        /// hop, not the eventual page.
        /// </summary>
        protected static async Task<HttpResponseMessage> GetRaw(
            HttpClient client, string relativePath)
        {
            Assert.NotNull(client);
            var request = new HttpRequestMessage(HttpMethod.Get, relativePath);
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }
        /// <summary>
        /// Smoke assertion: a GET on <paramref name="relativePath"/>
        /// returns 2xx (page served) or 3xx (redirect to login) or
        /// 401/403 (anonymous rejected by [Authorize]). Anything else
        /// — 404 (route missing), 5xx (server crash), connection
        /// refused (host not started) — fails the test.
        /// </summary>
        protected static async Task AssertResponds(
            HttpClient client, string relativePath)
        {
            var response = await GetRaw(client, relativePath);
            var status = (int)response.StatusCode;
            Assert.True(
                status >= 200 && status < 400 || status == 401 || status == 403,
                $"GET {relativePath} returned {status} {response.StatusCode}, " +
                "expected 2xx/3xx (page or redirect) or 401/403 (auth required).");
        }

        // FIXME write a scenario from an empty database [Fact]
        public void GitClone()
        {
            using var scope = _serverFixture.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.NotNull(dbContext.Project);
            Project yavsc = new Project
            {
                Name = "Yavsc"
            };
            dbContext.Project.Add(yavsc);
            dbContext.SaveChanges();
            var firstProject = dbContext.Project.Include(p => p.Repository).FirstOrDefault(
                p => p.Name == "Yavsc"
            );
            Assert.NotNull(firstProject);
            var di = new DirectoryInfo(_serverFixture.SiteSettings.GitRepository);
            if (!di.Exists) di.Create();

            var clone = new GitClone(_serverFixture.SiteSettings.GitRepository);
            clone.Launch(firstProject);
            gitRepo = di.FullName;
        }
        string gitRepo = null;
        private IConfigurationRoot configurationRoot;


        [Fact]
        public void HaveConfigurationRoot()
        {
            var builder = new ConfigurationBuilder();
            configurationRoot = builder.Build();
        }

        public void Dispose()
        {
            if (gitRepo != null)
            {
                Directory.Delete(Path.Combine(gitRepo, "yavsc"), true);
            }
        }
    }
}
