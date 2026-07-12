using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Yavsc.Org.Tests
{
    [Collection("Yavsc Server")]
    [Trait("regression", "oui")]
    public class Remoting : BaseTestContext, IClassFixture<WebServerFixture>
    {
        public Remoting(WebServerFixture serverFixture, ITestOutputHelper output)
        : base(output, serverFixture)
        {

        }


        [Fact]
        public async Task ObtainServiceToken()
        {
            var serverUrl = _serverFixture.Addresses.FirstOrDefault(u => u.StartsWith("https:"));
            if (string.IsNullOrEmpty(serverUrl))
                throw new InvalidOperationException("No HTTPS server address found");

            HttpClient client = NewHttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(serverUrl);
            if (disco.IsError)
            {
                // Diagnostic 2026-07-12 : capture the raw HTTP response
                // AND dump the OIDC-related DB state so we can pinpoint
                // which state is corrupt when the discovery is broken.
                var rawResp = await client.GetAsync(serverUrl + "/.well-known/openid-configuration");
                var body = await rawResp.Content.ReadAsStringAsync();

                string dbState = "no logger";
                try
                {
                    using var scope = _serverFixture.Services.CreateScope();
                    var cfg = scope.ServiceProvider
                        .GetRequiredService<IdentityServer8.EntityFramework.DbContexts.ConfigurationDbContext>();
                    var clients = cfg.Clients.Select(c => new {
                        c.Id, c.ClientId, c.Enabled, c.RequireClientSecret
                    }).ToList();
                    var apiScopes = cfg.ApiScopes.Select(s => new { s.Name, s.Enabled }).ToList();
                    var apiResources = cfg.ApiResources.Select(r => new { r.Name, r.Enabled }).ToList();
                    var identityResources = cfg.IdentityResources.Select(r => new { r.Name, r.Enabled }).ToList();
                    dbState = $"clients={System.Text.Json.JsonSerializer.Serialize(clients)}\n" +
                              $"apiScopes={System.Text.Json.JsonSerializer.Serialize(apiScopes)}\n" +
                              $"apiResources={System.Text.Json.JsonSerializer.Serialize(apiResources)}\n" +
                              $"identityResources={System.Text.Json.JsonSerializer.Serialize(identityResources)}";
                }
                catch (Exception dumpEx)
                {
                    dbState = $"dump failed: {dumpEx.Message}";
                }

                throw new Exception(
                    $"disco.Error={disco.Error}\n" +
                    $"HTTP status={(int)rawResp.StatusCode}\n" +
                    $"Body[0..2000]:\n{body.Substring(0, Math.Min(2000, body.Length))}\n" +
                    $"---\n" +
                    $"OIDC DB state at failure:\n{dbState}");
            }

            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = _serverFixture.TestClientId,
                ClientSecret = _serverFixture.TestClientSecret,
                Scope = "test",
                GrantType = "client_credentials"
            });
            if (response.IsError) throw new Exception(response.Error);
        }

        private static HttpClient NewHttpClient()
        {
            return new HttpClient(new BypassSslValidationHandler());
        }

        [Fact]
        public async Task ObtainResourceOwnerPasswordToken()
        {
            var serverUrl = _serverFixture.Addresses.FirstOrDefault(u => u.StartsWith("https:"));
            if (string.IsNullOrEmpty(serverUrl))
                throw new InvalidOperationException("No HTTPS server address found");

            var client = NewHttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(serverUrl);
            if (disco.IsError) throw new Exception(disco.Error);

            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = _serverFixture.TestClientId,
                ClientSecret = _serverFixture.TestClientSecret,
                UserName = _serverFixture.TestingUserName,
                Password = _serverFixture.TestingUserPassword,
                Scope = "test",
                Parameters =
                {
                    { "acr_values", "tenant:custom_account_store1 foo bar quux" }
                }
            });

            if (response.IsError) throw new Exception(response.Error);

        }

        public static IEnumerable<object[]> GetLoginIntentData()
        {
            return new object[][] { new object[] { "testuser", "test" } };
        }

    }

    internal class BypassSslValidationHandler : HttpClientHandler
    {
        public BypassSslValidationHandler()
        {
            // Override validation for this handler only
            ServerCertificateCustomValidationCallback = ValidateCertificate;
        }

        private bool ValidateCertificate(
            HttpRequestMessage request,
            X509Certificate2? certificate,
            X509Chain? chain,
            SslPolicyErrors errors)
        {
            // Accept all certificates (bypass validation)
            return true;
        }
    }
}
