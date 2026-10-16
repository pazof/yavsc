using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Markdown.Avalonia.Utils;
using Avalonia.Logging;

namespace PostIt.Services;

public sealed class BlogImageResolver : IPathResolver
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(15) };
    private readonly HttpClient _client;

    public BlogImageResolver() : this(Client) { }

    public BlogImageResolver(HttpClient client) => _client = client;

    public string? AssetPathRoot { get; set; }
    public IEnumerable<string>? CallerAssemblyNames { get; set; }

    public async Task<Stream?> ResolveImageResource(string relativeOrAbsolutePath)
    {
        var uri = !relativeOrAbsolutePath.StartsWith("/", StringComparison.Ordinal)
            && Uri.TryCreate(relativeOrAbsolutePath, UriKind.Absolute, out var absolute)
            ? absolute
            : new Uri(new Uri(AssetPathRoot ?? throw new InvalidOperationException(
                "La base des fichiers de l'auteur est indisponible.")), relativeOrAbsolutePath);
        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            throw new InvalidOperationException("Les images du billet doivent utiliser HTTP ou HTTPS.");
        try
        {
            // The Markdown decoder needs a seekable stream positioned at the start.
            return new MemoryStream(await _client.GetByteArrayAsync(uri), writable: false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            Logger.TryGet(LogEventLevel.Error, "BlogImages")?.Log(this,
                "Unable to load blog image {Url}: {Error}", uri.AbsoluteUri, ex.Message);
            throw;
        }
    }
}
