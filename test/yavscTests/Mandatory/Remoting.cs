using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using isnd.tests;
using Xunit.Abstractions;
using IdentityModel.Client;

namespace yavscTests
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
            if (disco.IsError) throw new Exception(disco.Error);

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
