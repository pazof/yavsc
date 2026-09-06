using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Services;

public sealed class NominatimReverseGeocodingService : IReverseGeocodingService
{
    private static readonly Uri BaseUri = new("https://nominatim.openstreetmap.org/");
    private readonly HttpClient _httpClient;

    public NominatimReverseGeocodingService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? CreateDefaultClient();
    }

    public async Task<string?> TryResolveAddressAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var requestUri = BuildReverseUri(latitude, longitude);

        try
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            return FormatAddress(json.RootElement);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private static HttpClient CreateDefaultClient()
    {
        var client = new HttpClient
        {
            BaseAddress = BaseUri,
            Timeout = TimeSpan.FromSeconds(10),
        };
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PostIt", "1.1"));
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr-FR"));
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr", 0.9));
        return client;
    }

    private static Uri BuildReverseUri(double latitude, double longitude)
    {
        var lat = latitude.ToString("0.######", CultureInfo.InvariantCulture);
        var lon = longitude.ToString("0.######", CultureInfo.InvariantCulture);
        var path = $"reverse?format=jsonv2&addressdetails=1&accept-language=fr&zoom=18&lat={lat}&lon={lon}";
        return new Uri(path, UriKind.Relative);
    }

    private static string? FormatAddress(JsonElement root)
    {
        if (root.TryGetProperty("address", out var address))
        {
            var street = JoinNonEmpty(
                TryGetString(address, "house_number"),
                TryGetString(address, "road"));

            var locality = JoinNonEmpty(
                TryGetString(address, "postcode"),
                TryGetString(address, "city")
                    ?? TryGetString(address, "town")
                    ?? TryGetString(address, "village")
                    ?? TryGetString(address, "municipality"));

            var formatted = JoinNonEmpty(street, locality);
            if (!string.IsNullOrWhiteSpace(formatted))
                return formatted;
        }

        if (root.TryGetProperty("display_name", out var displayName))
        {
            var value = displayName.GetString();
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
            ? property.GetString()
            : null;
    }

    private static string? JoinNonEmpty(params string?[] values)
    {
        List<string>? parts = null;
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            parts ??= new List<string>();
            parts.Add(value.Trim());
        }

        return parts is null || parts.Count == 0 ? null : string.Join(", ", parts);
    }
}