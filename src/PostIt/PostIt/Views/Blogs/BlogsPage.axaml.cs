
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
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
    private bool? _isWideLayout;

    public BlogsPage()
    {
        InitializeComponent();
        SizeChanged += (_, _) =>
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            UpdateResponsiveLayout();
        });
        DataContextChanged += (_, _) => UpdateHyperlinkCommand();
        UpdateHyperlinkCommand();
    }

    private void UpdateResponsiveLayout()
    {
        var size = TopLevel.GetTopLevel(this)?.ClientSize ?? Bounds.Size;
        if (size.Width <= 0 || size.Height <= 0)
            return;

        // Use the window size so compacting the navigation bar cannot change the orientation.
        var isWide = size.Width >= size.Height;
        if (_isWideLayout == isWide)
            return;
        _isWideLayout = isWide;

        MainGrid.RowDefinitions = new RowDefinitions(isWide ? "Auto,Auto,*,Auto" : "Auto,Auto,*,4,2*,Auto");
        MainGrid.ColumnDefinitions = new ColumnDefinitions(isWide ? "2*,4,*" : "*");
        var sidebarColumn = isWide ? 2 : 0;
        SetArea(BlogsHeaderPanel, 0, sidebarColumn);
        SetArea(BlogsCommandBar, 1, sidebarColumn);
        SetArea(PostsListBorder, 2, sidebarColumn);
        SetArea(PostsSplitter, isWide ? 0 : 3, isWide ? 1 : 0, isWide ? 3 : 1);
        SetArea(PostReader, isWide ? 0 : 4, 0, isWide ? 3 : 1);
        SetArea(PostEditor, isWide ? 0 : 4, 0, isWide ? 3 : 1);
        SetArea(StatusBar, isWide ? 3 : 5, 0, columnSpan: isWide ? 3 : 1);
        PostsSplitter.ResizeDirection = isWide ? GridResizeDirection.Columns : GridResizeDirection.Rows;
        PostsSplitter.HorizontalAlignment = isWide ? HorizontalAlignment.Center : HorizontalAlignment.Stretch;
        PostsSplitter.VerticalAlignment = isWide ? VerticalAlignment.Stretch : VerticalAlignment.Center;
        PostsSplitter.Margin = isWide ? new Thickness(0, 12) : new Thickness(12, 0);
        NavigationPage.SetBarHeightOverride(this, isWide ? 36 : null);
    }

    private static void SetArea(Control control, int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        Grid.SetRow(control, row);
        Grid.SetColumn(control, column);
        Grid.SetRowSpan(control, rowSpan);
        Grid.SetColumnSpan(control, columnSpan);
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
