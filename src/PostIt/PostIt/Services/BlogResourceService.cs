using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Abstract.Files;

namespace PostIt.Services;

public sealed class BlogResourceService
{
    private static readonly HttpClient SharedClient = new() { Timeout = TimeSpan.FromSeconds(15) };
    private readonly HttpClient _client;

    public BlogResourceService() : this(SharedClient) { }

    public BlogResourceService(HttpClient client) => _client = client;

    public static Uri GetUserBaseUri(string authority, string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new InvalidOperationException("Le login de l'auteur est nécessaire pour résoudre ses fichiers.");
        return FileServerUrlHelpers.GetUserFilesUri(GetAuthority(authority), Uri.EscapeDataString(login) + "/");
    }

    public static Uri Resolve(string link, string authority, string login)
    {
        if (string.IsNullOrWhiteSpace(link))
            throw new ArgumentException("Le lien est vide.", nameof(link));
        var origin = GetAuthority(authority);
        var uri = link.StartsWith("/", StringComparison.Ordinal)
            ? new Uri(origin, link)
            : Uri.TryCreate(link, UriKind.Absolute, out var absolute)
                ? absolute
                : new Uri(GetUserBaseUri(authority, login), link);
        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            throw new InvalidOperationException("Seuls les liens HTTP et HTTPS sont pris en charge.");
        return uri;
    }

    public static bool IsUserResource(Uri uri, string authority)
    {
        var root = FileServerUrlHelpers.GetUserFilesBaseUri(GetAuthority(authority));
        if (!root.IsBaseOf(uri) || !string.IsNullOrEmpty(uri.UserInfo))
            return false;
        var path = uri.AbsolutePath[root.AbsolutePath.Length..];
        var separator = path.IndexOf('/');
        return separator > 0 && separator < path.Length - 1;
    }

    public async Task<string> GetMimeTypeAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        using var head = new HttpRequestMessage(HttpMethod.Head, uri);
        using var response = await _client.SendAsync(head, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (response.StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotImplemented)
        {
            using var get = new HttpRequestMessage(HttpMethod.Get, uri);
            get.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 0);
            using var fallback = await _client.SendAsync(get, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            fallback.EnsureSuccessStatusCode();
            return GetMimeType(fallback, uri);
        }
        response.EnsureSuccessStatusCode();
        return GetMimeType(response, uri);
    }

    private static string GetMimeType(HttpResponseMessage response, Uri uri)
    {
        var mime = response.Content.Headers.ContentType?.MediaType;
        if (!string.IsNullOrEmpty(mime) && !mime.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            return mime;
        return System.IO.Path.GetExtension(uri.AbsolutePath).ToLowerInvariant() switch
        {
            ".mp4" or ".m4v" => "video/mp4",
            ".webm" => "video/webm",
            ".ogv" => "video/ogg",
            ".ogg" => "audio/ogg",
            ".mov" => "video/quicktime",
            ".mkv" => "video/x-matroska",
            ".avi" => "video/x-msvideo",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => mime ?? "application/octet-stream"
        };
    }

    private static Uri GetAuthority(string authority)
    {
        if (!Uri.TryCreate(authority, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            || !string.IsNullOrEmpty(uri.UserInfo))
            throw new InvalidOperationException("L'URL de Yavsc.Org est invalide dans les paramètres d'authentification.");
        return new Uri(uri.GetLeftPart(UriPartial.Authority) + "/");
    }
}
