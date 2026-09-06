using System.Net;
using System.Text;
using PostIt.Services;

namespace PostIt.Tests;

public class NominatimReverseGeocodingServiceTests
{
    [Fact]
    public async Task TryResolveAddressAsync_formats_compact_street_address_from_nominatim_payload()
    {
        var handler = new StubHandler("""
            {
              "display_name": "6, Place de l'Hôtel-de-Ville - Esplanade de la Libération, Paris, 75004, France",
              "address": {
                "house_number": "6",
                "road": "Place de l'Hôtel-de-Ville - Esplanade de la Libération",
                "postcode": "75004",
                "city": "Paris"
              }
            }
            """);

        var service = new NominatimReverseGeocodingService(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://nominatim.openstreetmap.org/")
        });

        var result = await service.TryResolveAddressAsync(48.8566, 2.3522);

        Assert.Equal("6, Place de l'Hôtel-de-Ville - Esplanade de la Libération, 75004, Paris", result);
        Assert.NotNull(handler.LastRequest);
        Assert.Contains("reverse?format=jsonv2", handler.LastRequest!.RequestUri!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task TryResolveAddressAsync_returns_null_on_unsuccessful_response()
    {
        var handler = new StubHandler("{}", HttpStatusCode.TooManyRequests);
        var service = new NominatimReverseGeocodingService(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://nominatim.openstreetmap.org/")
        });

        var result = await service.TryResolveAddressAsync(48.8566, 2.3522);

        Assert.Null(result);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly string _payload;
        private readonly HttpStatusCode _statusCode;

        public HttpRequestMessage? LastRequest { get; private set; }

        public StubHandler(string payload, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _payload = payload;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_payload, Encoding.UTF8, "application/json")
            });
        }
    }
}