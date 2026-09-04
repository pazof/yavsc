

using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;

namespace PostIt.Views;
public partial class SettingsPage: ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void ChooseAvatar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choisir un avatar",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Images")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.webp", "*.gif" }
                }
            }
        });

        var file = files.FirstOrDefault();
        if (file is null)
        {
            return;
        }

        try
        {
            await using var stream = await file.OpenReadAsync();
            var api = ((App)App.Current!).ServiceProvider!.GetRequiredService<YavscApiClient>();
            await api.SetAvatarAsync(stream, file.Name, GetMimeType(file.Name));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"🩎 Avatar upload failed: {ex.Message}");
        }
    }

    private static string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
