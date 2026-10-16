
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Markdown.Avalonia;

namespace PostIt.Views.Blogs;

public partial class BlogsPage : ContentPage
{
    private static readonly RelayCommand<string?> DisabledLinkCommand =
        new(static _ => { }, static _ => false);

    public BlogsPage()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => UpdateHyperlinkCommand();
        UpdateHyperlinkCommand();
    }

    private void UpdateHyperlinkCommand()
    {
        if (ArticleReader.Engine is IMarkdownEngine2 engine)
        {
            if (DataContext is ViewModels.BlogsViewModel vm)
                engine.HyperlinkCommand = vm.OpenResourceCommand;
            else
                engine.HyperlinkCommand = DisabledLinkCommand;
        }
    }

    protected override async void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (DataContext is ViewModels.BlogsViewModel vm)
        {
            await vm.InitializeAsync();
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
