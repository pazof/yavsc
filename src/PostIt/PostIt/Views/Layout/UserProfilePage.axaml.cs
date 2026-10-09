using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class UserProfilePage : ContentPage
{
    public UserProfilePage()
    {
        InitializeComponent();
    }

    private async void ChooseAvatar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not UserProfilePageViewModel viewModel)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            viewModel.StatusMessage = "Impossible d’accéder au sélecteur de fichiers.";
            return;
        }

        viewModel.IsBusy = true;
        try
        {
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choisir un avatar",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Images")
                    {
                        Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp", "*.gif", "*.bmp", "*.tif", "*.tiff"]
                    }
                ]
            });

            var file = files.FirstOrDefault();
            if (file is null)
            {
                return;
            }

            var properties = await file.GetBasicPropertiesAsync();
            if (properties.Size is > 2 * 1024 * 1024)
            {
                viewModel.StatusMessage = "L’avatar ne doit pas dépasser 2 Mo.";
                return;
            }

            var app = (App?)Application.Current;
            var api = app?.ServiceProvider?.GetRequiredService<YavscApiClient>();
            if (api is null)
            {
                viewModel.StatusMessage = "Client API indisponible.";
                return;
            }

            await using var stream = await file.OpenReadAsync();
            var message = await api.SetAvatarAsync(stream, file.Name, GetMimeType(file.Name));
            await viewModel.RefreshAsync();
            if (viewModel.IsProfileLoaded)
            {
                viewModel.StatusMessage = message;
            }
        }
        catch (Exception ex)
        {
            viewModel.StatusMessage = $"Impossible d’envoyer l’avatar : {ex.Message}";
        }
        finally
        {
            viewModel.IsBusy = false;
        }
    }

    private async void OpenCircles_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var app = (App?)Application.Current;
        var viewModel = app?.ServiceProvider?.GetRequiredService<CirclesPageViewModel>();
        if (app is null || viewModel is null)
        {
            if (DataContext is UserProfilePageViewModel profile)
            {
                profile.StatusMessage = "La page des cercles est indisponible.";
            }
            return;
        }

        await viewModel.RefreshAsync();
        await app.PushPageAsync(viewModel);
    }

    private async void OpenPerformerConfiguration_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var app = (App?)Application.Current;
        var viewModel = app?.ServiceProvider?.GetRequiredService<PerformerConfigurationPageViewModel>();
        if (app is null || viewModel is null)
        {
            if (DataContext is UserProfilePageViewModel profile)
            {
                profile.StatusMessage = "La configuration professionnelle est indisponible.";
            }
            return;
        }

        await viewModel.InitializeAsync();
        await app.PushPageAsync(viewModel);
    }

    private async void OpenBlogs_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await App.PushBlogsPageAsync();
    }

    private static string GetMimeType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".tif" or ".tiff" => "image/tiff",
            _ => "application/octet-stream"
        };
}
