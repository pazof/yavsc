
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Controls.Platform;

namespace PostIt.Views.Blogs;

public partial class BlogsPage : ContentPage
{
    private IInputPane? _inputPane;

    public BlogsPage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // Récupère l'accès au TopLevel de la fenêtre/application
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            _inputPane = topLevel!.InputPane;
            if (_inputPane != null)
            {
                // S'abonne aux changements d'état du clavier virtuel
                _inputPane.StateChanged += OnInputPaneStateChanged;

                // Applique la configuration initiale si le clavier est déjà présent
                UpdateLayoutForKeyboard(_inputPane.OccludedRect.Height);
            }
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (_inputPane != null)
        {
            _inputPane.StateChanged -= OnInputPaneStateChanged;
        }
        base.OnUnloaded(e);
    }


   private void UpdateLayoutForKeyboard(double keyboardHeight)
    {
        double visibleHeight = this.Bounds.Height - keyboardHeight;
        if (keyboardHeight > 0)
        {
            DraftArticleTextEditor.MaxHeight = visibleHeight;
        }
        else
        {
            DraftArticleTextEditor.MaxHeight = double.PositiveInfinity;
        }
    }

    private void OnInputPaneStateChanged(object? sender, InputPaneStateEventArgs e)
    {
        // e.EndRect contient le rectangle final du clavier virtuel
        // e.EndRect.Height vous donne sa hauteur exacte en pixels indépendants du périphérique (pixels logiques)
        UpdateLayoutForKeyboard(e.EndRect.Height);
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
