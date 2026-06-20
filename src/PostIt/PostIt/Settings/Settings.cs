using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using IdentityModel.OidcClient;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PostIt;

public partial class Settings : ObservableObject
{
    const string SettingsFileName = "postit-settings.json";
    IStorageFolder? folder = null;

    [ObservableProperty]
    public partial AuthenticationSettings Authentication { get; set; } = new();

    [ObservableProperty]
    public partial bool DarkMode { get; set; } = false;

    [ObservableProperty]
    public partial string ApiUrl { get; set; } = "https://blogs.pschneider.fr/api/v1/";


    [ObservableProperty]
    public partial string[] Scopes { get; set; }

    internal OidcClientOptions GetOidcClientOptions()
    {
        return new OidcClientOptions
        {
            Authority = Authentication.Authority,
            ClientId = Authentication.ClientId,
            ClientSecret = Authentication.ClientSecret,
            Scope = string.Join(' ', this.Scopes)
        };

    }

    internal async Task Load()
    {
        string configDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "PostIt"
);
        Directory.CreateDirectory(configDir);

        string configPath = Path.Combine(configDir, SettingsFileName);

        FileInfo configFileInfo = new FileInfo(configPath);

        if (!configFileInfo.Exists)
        {
            Console.Error.WriteLine($"🩎 Settings file not found at {configFileInfo.FullName}");
            return; // no settings file
        }

        Console.WriteLine($"🔎 Loading settings from {configFileInfo.FullName}");

        try
        {
            using var stream = configFileInfo.OpenRead();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.Error.WriteLine("🩎 Settings file is empty.");
                return;
            }

            var settings = JsonSerializer.Deserialize<Settings>(json);

            if (settings is null)
            {
                Console.Error.WriteLine("🩎 Settings file is invalid.");
                return;
            }
            this.Authentication = settings.Authentication;
            this.DarkMode = settings.DarkMode;
            this.ApiUrl = settings.ApiUrl;
            this.Scopes = settings.Scopes;

        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"🩎 Error loading settings: {ex.Message}");
        }
    }
}
