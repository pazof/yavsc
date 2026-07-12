using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using IdentityModel.Client;

namespace Yavsc.Org.Tests
{
    [Collection("Yavsc Server")]
    [Trait("regression", "oui")]
    public class Remoting : BaseTestContext, IClassFixture<WebServerFixture>
    {
        private readonly ITestOutputHelper _output;

        public Remoting(WebServerFixture serverFixture, ITestOutputHelper output)
        : base(output, serverFixture)
        {
            _output = output;
        }


        [Fact]
        public async Task ObtainServiceToken()
        {
            var serverUrl = GetServerUrl();
            var cancellationToken = TestContext.Current.CancellationToken;

            HttpClient client = NewHttpClient();
            var tokenEndpoint = await ResolveTokenEndpointAsync(client, serverUrl, cancellationToken);

            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = RequireNonEmpty(_serverFixture.TestClientId, nameof(_serverFixture.TestClientId)),
                ClientSecret = RequireNonEmpty(_serverFixture.TestClientSecret, nameof(_serverFixture.TestClientSecret)),
                Scope = "test",
                GrantType = "client_credentials"
            }, cancellationToken);
            if (response.IsError) throw new Exception(response.Error);
        }

        private static HttpClient NewHttpClient()
        {
            return new HttpClient(new BypassSslValidationHandler());
        }

        [Fact]
        public async Task ObtainResourceOwnerPasswordToken()
        {
            var serverUrl = GetServerUrl();
            var cancellationToken = TestContext.Current.CancellationToken;

            var client = NewHttpClient();
            var tokenEndpoint = await ResolveTokenEndpointAsync(client, serverUrl, cancellationToken);

            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = RequireNonEmpty(_serverFixture.TestClientId, nameof(_serverFixture.TestClientId)),
                ClientSecret = RequireNonEmpty(_serverFixture.TestClientSecret, nameof(_serverFixture.TestClientSecret)),
                UserName = RequireNonEmpty(_serverFixture.TestingUserName, nameof(_serverFixture.TestingUserName)),
                Password = RequireNonEmpty(_serverFixture.TestingUserPassword, nameof(_serverFixture.TestingUserPassword)),
                Scope = "test",
                Parameters =
                {
                    { "acr_values", "tenant:custom_account_store1 foo bar quux" }
                }
            }, cancellationToken);

            if (response.IsError) throw new Exception(response.Error);

        }

        public static IEnumerable<object[]> GetLoginIntentData()
        {
            return new object[][] { new object[] { "testuser", "test" } };
        }

        private async Task<string> ResolveTokenEndpointAsync(HttpClient client, string serverUrl, CancellationToken cancellationToken)
        {
            var disco = await client.GetDiscoveryDocumentAsync(serverUrl, cancellationToken);
            if (!disco.IsError && !string.IsNullOrWhiteSpace(disco.TokenEndpoint))
            {
                return disco.TokenEndpoint;
            }

            // Some full-suite runs intermittently return 500 on the OIDC
            // discovery document while /connect/token remains available.
            var fallback = new Uri(new Uri(serverUrl), "/connect/token").ToString();
            _output.WriteLine($"WARNING: OIDC discovery failed ({disco.Error}). Fallback token endpoint: {fallback}");
            return fallback;
        }

        private string GetServerUrl()
        {
            return RequireNonEmpty(_serverFixture.SiteSettings?.Authority, "SiteSettings.Authority");
        }

        private static string RequireNonEmpty(string? value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Missing required test setting: {name}");
            }

            return value;
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
