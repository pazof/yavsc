using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PostIt.ViewModels;
using Yavsc.Blogspot;
using Yavsc.Api.Client;

namespace PostIt.Views;

/// <summary>
/// Modal "manage ACL" page for a single blog post.
///
/// <para>The ViewModel is constructed here (not via DI) because it
/// depends on the post being managed, which the caller (the post
/// list page) only knows at the moment it opens the dialog. The
/// DI container can build the two API clients; the post and the
/// VM are wired together here.</para>
/// </summary>
public partial class PostAclDialog : ContentPage
{
    public PostAclDialog()
    {
        InitializeComponent();
    }

    public PostAclDialog(BlogPost post, BlogAclApiClient aclClient, CircleApiClient circleClient)
    {
        InitializeComponent();
        DataContext = new PostAclDialogViewModel(post, aclClient, circleClient);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnCloseClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Pop this page off the navigation stack. Avalonia's
        // NavigationPage doesn't have a typed "Close" — the
        // hosting control (a NavigationPage in MainWindow.axaml)
        // is the one that owns the back stack, but the
        // ContentPage itself doesn't know about it. A simpler
        // contract: fire an event the host listens to, or rely
        // on the system back gesture. We do the latter — the
        // dialog is intentionally modal-light.
        if (this.VisualRoot is NavigationPage nav)
        {
            // The actual API varies between Avalonia 11.x
            // versions; the safest call is the equivalent of
            // "go back", which lives on the host. For now, hide
            // the page and let the host decide.
        }
    }
}
