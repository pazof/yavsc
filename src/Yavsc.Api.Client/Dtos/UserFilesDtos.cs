using System;
using System.Collections.Generic;

namespace Yavsc.Api.Client;

/// <summary>
/// Représentation d'un répertoire de l'espace de stockage personnel,
/// renvoyée par <c>GET api/v1/fs/{subdir}</c> (host Blogs). Ne dépend pas
/// des helpers serveur : c'est un pur DTO de désérialisation.
/// </summary>
public sealed class UserDirectoryDto
{
    public string? UserName { get; set; }
    public string? SubPath { get; set; }
    public FileEntryDto[] Files { get; set; } = Array.Empty<FileEntryDto>();
    public DirEntryDto[] SubDirectories { get; set; } = Array.Empty<DirEntryDto>();
}

public sealed class FileEntryDto
{
    /// <summary>Identifiant de la ligne UploadedFile, pour l'attachement par référence.</summary>
    public long? Id { get; set; }
    public string? Name { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
}

public sealed class DirEntryDto
{
    public string? Name { get; set; }
}

/// <summary>
/// Métadonnées d'une pièce jointe (fichier de l'espace perso rattaché à
/// un devis ou une demande), renvoyées par les endpoints d'attachement.
/// </summary>
public sealed class AttachmentDto
{
    public long FileId { get; set; }
    /// <summary>Chemin relatif dans l'espace perso du propriétaire.</summary>
    public string? Path { get; set; }
    public long Size { get; set; }
    public string? ContentType { get; set; }
    public string? OwnerId { get; set; }
}

/// <summary>
/// Métadonnées d'un fichier reçu après un téléversement multipart vers
/// <c>POST api/v1/fs/{subdir}</c>. Porte notamment l'identifiant
/// <see cref="FileId"/> de la ligne UploadedFile créée côté serveur.
/// </summary>
public sealed class FileReceivedDto
{
    public long? FileId { get; set; }
    public string? FileName { get; set; }
    public long Length { get; set; }
    public string? ContentType { get; set; }
    public bool QuotaOffense { get; set; }
    public bool Overridden { get; set; }
}

/// <summary>
/// Contenu d'un fichier à téléverser (nom, octets, type MIME), à l'image
/// de <see cref="BlogUploadFile"/> mais pour l'espace perso générique.
/// </summary>
public sealed record UploadFile(string FileName, byte[] Content, string? ContentType);