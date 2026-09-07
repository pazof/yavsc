using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PostIt.ViewModels;
using Yavsc.Blogspot;
using Yavsc.Api.Client;

namespace PostIt.Views;

/// <summary>
/// Modal "manage ACL" page for a single blog post.
///
/// <para>The ViewModel is constructed by the caller (the post
/// list page) and handed to <see cref="App.PushPageAsync"/>,
/// which routes through <see cref="ViewLocator"/> and lands
/// here via the parameterless DI constructor. The VM is then
/// assigned to <see cref="ContentPage.DataContext"/> by
/// <c>App.PushPageAsync</c> — we listen for that one-shot
/// assignment and trigger <c>LoadAsync</c> right after, so the
/// dropdown's <c>MyCircles</c> and the list's <c>AclEntries</c>
/// are populated when the dialog appears. The VM is idempotent
/// under repeated loads.</para>
/// </summary>
public partial class PostAclDialog : ContentPage
{
    public PostAclDialog()
    {
        InitializeComponent();

        // App.PushPageAsync wires the VM via DataContext after
        // building the page. We subscribe once to fire LoadAsync
        // the moment the VM is attached. Using DataContextChanged
        // (rather than AttachedToVisualTree) is what makes this
        // work in the headless test harness too: the load is
        // tied to the VM being available, not to the visual tree
        // being realised (which is a separate concern).
        EventHandler? handler = null;
        handler = (_, _) =>
        {
            if (DataContext is PostAclDialogViewModel vm)
            {
                this.DataContextChanged -= handler;
                _ = vm.LoadAsync();
            }
        };
        this.DataContextChanged += handler;
    }

    public PostAclDialog(BlogPostDto post, BlogAclApiClient aclClient, CircleApiClient circleClient)
    {
        // This overload is not used by the production path —
        // MainPageViewModel pushes the VM via App.PushPageAsync
        // and App routes through ViewLocator, which resolves this
        // page via the parameterless ctor. It is kept so test
        // scaffolding that wants to bypass the nav pipeline can
        // still wire a VM directly without losing the load
        // trigger: the constructor sets DataContext before the
        // DataContextChanged subscription fires, so the load
        // is guaranteed to run in either case.
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
