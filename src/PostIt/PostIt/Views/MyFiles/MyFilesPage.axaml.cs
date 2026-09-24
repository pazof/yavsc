using System;
using System.IO;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class MyFilesPage : ContentPage
{
    public MyFilesPage()
    {
        InitializeComponent();
    }

    private async void Upload_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null || DataContext is not MyFilesViewModel vm)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choisir des fichiers à téléverser dans Mes fichiers",
            AllowMultiple = true,
        });

        if (files.Count == 0)
            return;

        var uploads = new List<Yavsc.Api.Client.UploadFile>(files.Count);
        foreach (var file in files)
        {
            await using var stream = await file.OpenReadAsync();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            uploads.Add(new Yavsc.Api.Client.UploadFile(file.Name, memory.ToArray(), GetMimeType(file.Name)));
        }

        await vm.UploadAsync(uploads);
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
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}