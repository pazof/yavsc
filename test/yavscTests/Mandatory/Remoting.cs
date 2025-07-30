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

        [Theory]
        [MemberData(nameof(GetLoginIntentData))]
        public async Task TestUserMayLogin
            (
             string userName,
             string password
            )
        {

        }

        [Fact]
        public async Task ObtainServiceToken()
        {
            var serverUrl = _serverFixture.Addresses.FirstOrDefault(
                u => u.StartsWith("https:")
            );

            String authority = _serverFixture.SiteSettings.Authority;
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(authority);
            if (disco.IsError) throw new Exception(disco.Error);

            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "m2m.client", // _serverFixture.TestClientId,
                ClientSecret = "511536EF-F270-4058-80CA-1C89C192F69A" //_serverFixture.TestClientSecret,
            });
/*"mvc";
        options.ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0";*/
            if (response.IsError) throw new Exception(response.Error);

        }

   [Fact]
        public async Task ObtainResourceOwnerPasswordToken()
        {
            var serverUrl = _serverFixture.Addresses.FirstOrDefault(
                u => u.StartsWith("https:")
            );

            String authority = _serverFixture.SiteSettings.Authority;
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(authority);
            if (disco.IsError) throw new Exception(disco.Error);

            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
    {
        Address = disco.TokenEndpoint,

        ClientId = "m2m.client",
        ClientSecret = "511536EF-F270-4058-80CA-1C89C192F69A",

        UserName = _serverFixture.TestingUserName,
        Password = _serverFixture.TestingUserPassword,

        Scope = "scope1",

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
}
