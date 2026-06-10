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
    const string SettingsFileName = "settings.json";
    IStorageFolder? folder = null;

    [ObservableProperty]
    public partial AuthenticationSettings Authentication { get; set; } = new();

    [ObservableProperty]
    public partial bool DarkMode { get; set; } = false;

    [ObservableProperty]
    public partial string ApiUrl { get; set; } = "https://blogs.pschneider.fr/api/v1/";


    [ObservableProperty]
    public partial string[] Scopes { get; set; }

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

    internal async Task Load(IStorageProvider storageProvider)
    {
        var configFile = await storageProvider.TryGetFileFromPathAsync(
            Path.Combine(AppContext.BaseDirectory, SettingsFileName));

        if (configFile is null)
        {
            Console.Error.WriteLine("🩎 No settings file found.");
            return; // no settings file
        }
        try {
            using var stream = await configFile.OpenReadAsync();

        
                    using var reader = new StreamReader( stream);
                    var json = await reader.ReadToEndAsync();
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Console.Error.WriteLine("🩎 Settings file is empty.");
                        return ;
                    }

                    var settings = JsonSerializer.Deserialize<Settings>(json);

                    if (settings is null)
                    {
                        Console.Error.WriteLine("🩎 Settings file is invalid.");
                        return ;
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
