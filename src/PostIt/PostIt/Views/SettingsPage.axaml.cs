

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;

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
        SetAvatarStatus("Sélection d'un fichier avatar...", Brushes.Gray);

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            SetAvatarStatus("Impossible d'accéder à la fenêtre active.", Brushes.IndianRed);
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
            SetAvatarStatus("Upload annulé.", Brushes.Gray);
            ChooseAvatarButton.IsEnabled = true;
            return;
        }

        try
        {
            SetAvatarStatus("Upload avatar en cours...", Brushes.Gray);

            await using var stream = await file.OpenReadAsync();
            var api = ((App)App.Current!).ServiceProvider!.GetRequiredService<YavscApiClient>();
            var message = await api.SetAvatarAsync(stream, file.Name, GetMimeType(file.Name));
            SetAvatarStatus(string.IsNullOrWhiteSpace(message)
                ? "Avatar mis à jour."
                : message, Brushes.ForestGreen);

            _ = ClearSuccessStatusLaterAsync(statusVersion);
        }
        catch (Exception ex)
        {
            SetAvatarStatus($"Échec upload avatar: {ex.Message}", Brushes.IndianRed);
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

        if (AvatarUploadStatusText.Foreground == Brushes.ForestGreen)
        {
            SetAvatarStatus($"Avatar prêt. Formats supportés: {AcceptedAvatarFormats}. Taille max: {MaxAvatarSizeMegabytes} MB.", Brushes.Gray);
        }
    }

    private void SetAvatarStatus(string message, IBrush color)
    {
        AvatarUploadStatusText.Foreground = color;
        AvatarUploadStatusText.Text = message;
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
