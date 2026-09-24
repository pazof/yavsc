using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Yavsc.Api.Client;

/// <summary>
/// Client HTTP de l'espace de stockage personnel
/// (<c>api/v1/fs</c>, servi par le host <c>Yavsc.Blogs</c>). Même forme
/// que <see cref="BlogApiClient"/> : le transport (URL de base, JSON,
/// Bearer, refresh silencieux sur 401) est délégué à
/// <see cref="IYavscApiClient"/> ; cette classe n'est qu'un mappeur
/// DTO↔chemin.
///
/// <para><b>Convention d'URL.</b> Le <c>pathPrefix</c> est relatif au
/// segment de version déjà porté par l'URL de base
/// (<c>…/api/v1/</c>) : <c>"fs"</c> résout en <c>…/api/v1/fs</c>, qui
/// correspond au <c>[Route(APIPrefix + "/fs")]</c> de
/// <c>FileSystemApiController</c>. Ne pas ré-inclure <c>api/</c>.</para>
/// </summary>
public sealed class UserFilesApiClient
{
    private const string PathPrefix = "fs";

    private readonly IYavscApiClient _api;
    private readonly Uri _baseAddress;

    public UserFilesApiClient(IYavscApiClient api, string blogsBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        if (string.IsNullOrEmpty(blogsBaseAddress))
            throw new ArgumentException("Base address is required.", nameof(blogsBaseAddress));

        _baseAddress = new Uri(blogsBaseAddress);
        _api.Http.BaseAddress = _baseAddress;
    }

    /// <summary>
    /// Liste le contenu du sous-répertoire <paramref name="subdir"/> de
    /// l'espace perso de l'utilisateur (<c>GET fs/{subdir}</c>).
    /// </summary>
    public Task<UserDirectoryDto> ListAsync(string? subdir = null, CancellationToken ct = default)
    {
        var path = string.IsNullOrWhiteSpace(subdir) ? PathPrefix : $"{PathPrefix}/{subdir.TrimStart('/')}";
        return _api.CallAsync<UserDirectoryDto>(HttpMethod.Get, path, ct: ct);
    }

    /// <summary>
    /// Téléverse des fichiers dans le sous-répertoire
    /// <paramref name="subdir"/> (<c>POST fs/{subdir}</c> multipart).
    /// Renvoie les métadonnées des fichiers reçus, dont l'identifiant
    /// <see cref="FileReceivedDto.FileId"/> servant à l'attachement.
    /// </summary>
    public Task<List<FileReceivedDto>> UploadAsync(string subdir, IReadOnlyCollection<UploadFile> files, CancellationToken ct = default)
    {
        if (files is null || files.Count == 0)
            throw new ArgumentException("At least one file is required.", nameof(files));

        var path = string.IsNullOrWhiteSpace(subdir)
            ? PathPrefix
            : $"{PathPrefix}/{subdir.TrimStart('/')}";

        return _api.CallAsync<List<FileReceivedDto>>(HttpMethod.Post, path, () => CreateMultipartContent(files), ct);
    }

    /// <summary>
    /// Supprime un fichier ou répertoire de l'espace perso
    /// (<c>DELETE fs/{path}</c>).
    /// </summary>
    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required.", nameof(path));
        return _api.CallAsync(HttpMethod.Delete, $"{PathPrefix}/{path.TrimStart('/')}", ct: ct);
    }

    /// <summary>
    /// Télécharge une pièce jointe référencée par son identifiant
    /// <see cref="FileReceivedDto.FileId"/> / <see cref="AttachmentDto.FileId"/>
    /// (<c>GET fs/file/{fileId}</c>). Renvoie les octets bruts ; le serveur
    /// (host Blogs) autorise l'appelant (propriétaire ou partie au
    /// devis/demande auquel le fichier est rattaché).
    /// </summary>
    public Task<byte[]> DownloadFileAsync(long fileId, CancellationToken ct = default)
    {
        if (fileId <= 0) throw new ArgumentOutOfRangeException(nameof(fileId));
        return _api.DownloadAsync(HttpMethod.Get, $"{PathPrefix}/file/{fileId}", ct);
    }

    /// <summary>
    /// Construit le corps <c>multipart/form-data</c> d'un téléversement :
    /// une part <c>file</c> par fichier (type MIME retombant sur
    /// <c>application/octet-stream</c>). Exposé <c>public</c> pour les
    /// tests backend qui assertent la forme filaire.
    /// </summary>
    public static HttpContent CreateMultipartContent(IReadOnlyCollection<UploadFile> files)
    {
        var content = new MultipartFormDataContent();
        foreach (var file in files)
        {
            var fileContent = new ByteArrayContent(file.Content);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            content.Add(fileContent, "file", file.FileName);
        }
        return content;
    }
}