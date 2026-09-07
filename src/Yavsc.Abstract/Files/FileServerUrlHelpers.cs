namespace Yavsc.Abstract.Files;

/// <summary>
/// Helpers pour dériver les URL publiques des fichiers statiques à partir
/// d'une URL d'autorité OIDC ou d'un autre point d'entrée racine.
/// </summary>
public static class FileServerUrlHelpers
{
    /// <summary>
    /// Dérive la racine publique des fichiers utilisateur en alignant
    /// le chemin sur <see cref="Yavsc.Constants.UserFilesPath"/>.
    /// </summary>
    /// <param name="authorityBaseUrl">
    /// URL absolue de base, typiquement l'autorité OIDC de Yavsc.Org.
    /// </param>
    /// <returns>Une URL absolue pointant vers la racine des fichiers utilisateur.</returns>
    public static Uri GetUserFilesBaseUri(Uri authorityBaseUrl)
    {
        ArgumentNullException.ThrowIfNull(authorityBaseUrl);

        if (!authorityBaseUrl.IsAbsoluteUri)
        {
            throw new ArgumentException(
                "The authority base URL must be absolute.",
                nameof(authorityBaseUrl));
        }

        var baseString = authorityBaseUrl.GetLeftPart(UriPartial.Authority);
        return new Uri(new Uri(baseString, UriKind.Absolute), EnsureTrailingSlash(Yavsc.Constants.UserFilesPath));
    }

    /// <summary>
    /// Dérive la racine publique des fichiers utilisateur en alignant
    /// le chemin sur <see cref="Yavsc.Constants.UserFilesPath"/>.
    /// </summary>
    /// <param name="authorityBaseUrl">
    /// URL absolue de base, typiquement l'autorité OIDC de Yavsc.Org.
    /// </param>
    /// <returns>Une URL absolue pointant vers la racine des fichiers utilisateur.</returns>
    public static Uri GetUserFilesBaseUri(string authorityBaseUrl)
        => GetUserFilesBaseUri(new Uri(authorityBaseUrl, UriKind.Absolute));

    /// <summary>
    /// Construit l'URL d'un fichier utilisateur à partir de la base d'autorité
    /// et d'un chemin relatif sous la racine des fichiers.
    /// </summary>
    public static Uri GetUserFilesUri(Uri authorityBaseUrl, string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var baseUri = GetUserFilesBaseUri(authorityBaseUrl);
        return new Uri(baseUri, NormalizeRelativePath(relativePath));
    }

    /// <summary>
    /// Construit l'URL d'un fichier utilisateur à partir de la base d'autorité
    /// et d'un chemin relatif sous la racine des fichiers.
    /// </summary>
    public static Uri GetUserFilesUri(string authorityBaseUrl, string relativePath)
        => GetUserFilesUri(new Uri(authorityBaseUrl, UriKind.Absolute), relativePath);

    private static string EnsureTrailingSlash(string path)
        => path.EndsWith("/", StringComparison.Ordinal) ? path : path + "/";

    private static string NormalizeRelativePath(string relativePath)
        => relativePath.TrimStart('/');
}
