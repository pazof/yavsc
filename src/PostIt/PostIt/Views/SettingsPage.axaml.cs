

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;
using PostIt.ViewModels;

namespace PostIt.Views;
public partial class SettingsPage: ContentPage
{
    private const int MaxAvatarSizeMegabytes = 2;
    private const string AcceptedAvatarFormats = "PNG, JPG/JPEG, WEBP, GIF";

    private int _avatarStatusVersion;

    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void ChooseAvatar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var statusVersion = Interlocked.Increment(ref _avatarStatusVersion);
        ChooseAvatarButton.IsEnabled = false;
        SetAvatarStatus("Selection d'un fichier avatar...", StatusSeverity.Info);

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            SetAvatarStatus("Impossible d'acceder a la fenetre active.", StatusSeverity.Error);
            ChooseAvatarButton.IsEnabled = true;
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
            SetAvatarStatus("Upload annule.", StatusSeverity.Warning);
            ChooseAvatarButton.IsEnabled = true;
            return;
        }

        try
        {
            SetAvatarStatus("Upload avatar en cours...", StatusSeverity.Info);

            await using var stream = await file.OpenReadAsync();
            var api = ((App)App.Current!).ServiceProvider!.GetRequiredService<YavscApiClient>();
            var message = await api.SetAvatarAsync(stream, file.Name, GetMimeType(file.Name));
            SetAvatarStatus(string.IsNullOrWhiteSpace(message)
                ? "Avatar mis a jour."
                : message,
                StatusSeverity.Info);

            _ = ClearSuccessStatusLaterAsync(statusVersion);
        }
        catch (Exception ex)
        {
            SetAvatarStatus($"Echec upload avatar: {ex.Message}", StatusSeverity.Error);
            Console.Error.WriteLine($"🩎 Avatar upload failed: {ex.Message}");
        }
        finally
        {
            ChooseAvatarButton.IsEnabled = true;
        }
    }

    private async Task ClearSuccessStatusLaterAsync(int statusVersion)
    {
        await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(true);
        if (statusVersion != _avatarStatusVersion)
            return;

        SetAvatarStatus($"Avatar pret. Formats supportes: {AcceptedAvatarFormats}. Taille max: {MaxAvatarSizeMegabytes} MB.", StatusSeverity.Info);
    }

    private void SetAvatarStatus(string message, StatusSeverity severity)
    {
        if (DataContext is Settings settings)
        {
            settings.SetActionStatus(message, severity);
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
