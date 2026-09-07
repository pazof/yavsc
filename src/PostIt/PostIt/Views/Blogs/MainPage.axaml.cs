using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Controls.Primitives;

namespace PostIt.Views.Blogs;

public partial class BlogsPage : ContentPage
{
    public BlogsPage()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (DataContext is ViewModels.BlogsViewModel vm)
        {
            if (!vm.IsLoaded)
            {
                vm.RefreshAsync().Wait();
            }
        }
    }

    private async void AddAttachment_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null || DataContext is not ViewModels.BlogsViewModel vm)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choisir des fichiers à joindre au billet",
            AllowMultiple = true,
        });

        if (files.Count == 0)
            return;

        var uploads = new System.Collections.Generic.List<Yavsc.Api.Client.BlogUploadFile>(files.Count);
        foreach (var file in files)
        {
            await using var stream = await file.OpenReadAsync();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            uploads.Add(new Yavsc.Api.Client.BlogUploadFile(file.Name, memory.ToArray(), GetMimeType(file.Name)));
        }

        vm.DraftAttachments.Clear();
        foreach (var upload in uploads)
            vm.DraftAttachments.Add(upload);
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
