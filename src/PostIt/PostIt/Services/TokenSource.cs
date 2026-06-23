using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostIt.Services;

/// <summary>
/// On-disk persistence for the OIDC token bundle (access + refresh + id).
///
/// Layout: a single JSON file. The file is written with 0600 on POSIX
/// systems; on Windows the OS-level DACL inherits from the user's
/// profile. Future hardening: route writes through libsecret / DPAPI
/// so the file itself is never readable in cleartext on disk.
/// </summary>
public sealed class TokenStore
{
    private readonly string _path;
    private readonly object _gate = new();

    public TokenStore(string path) => _path = path;

    public RefreshTokenRecord? Load()
    {
        lock (_gate)
        {
            if (!File.Exists(_path)) return null;
            var json = File.ReadAllText(_path);
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonSerializer.Deserialize<RefreshTokenRecord>(json);
        }
    }

    public void Save(RefreshTokenRecord record)
    {
        lock (_gate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var json = JsonSerializer.Serialize(record, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
            File.WriteAllText(_path, json);
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                File.SetUnixFileMode(_path,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            if (File.Exists(_path)) File.Delete(_path);
        }
    }
}

/// <summary>
/// Snapshot of the tokens the API client needs to keep a session alive
/// across process restarts. <see cref="AccessTokenExpiresAt"/> is the
/// absolute UTC time the access token is no longer valid; the API
/// client compares it to <see cref="DateTimeOffset.UtcNow"/> before
/// every call to decide whether to refresh.
/// </summary>
public sealed record RefreshTokenRecord(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    string? IdToken = null);
