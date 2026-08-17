using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using PostIt.ViewModels;
using Yavsc.Blogspot;
using Yavsc.Api.Client;

namespace PostIt.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    MainPageViewModel? _vm;

    void OnDataContextChanged(object? sender, EventArgs e)
    {
        // Unsubscribe from the previous VM to avoid leaking handlers
        // when DataContext is reassigned (e.g. by the navigation
        // host or a binding reset).
        if (_vm is not null)
        {
            _vm.ManageAclRequested -= OnManageAclRequested;
            _vm.OpenCirclesRequested -= OnOpenCirclesRequested;
        }
        _vm = DataContext as MainPageViewModel;
        if (_vm is not null)
        {
            _vm.ManageAclRequested += OnManageAclRequested;
            _vm.OpenCirclesRequested += OnOpenCirclesRequested;
        }
    }

    void OnManageAclRequested(object? sender, BlogPost post)
    {
        var app = Application.Current as App;
        var services = app?.ServiceProvider;
        if (services is null || post is null) return;

        var dialog = new PostAclDialog(
            post,
            services.GetRequiredService<BlogAclApiClient>(),
            services.GetRequiredService<CircleApiClient>());

        if (this.VisualRoot is MainWindow window)
            _ = window.NavRoot.PushAsync(dialog);
    }

    void OnOpenCirclesRequested(object? sender, EventArgs e)
    {
        var app = Application.Current as App;
        var services = app?.ServiceProvider;
        if (services is null) return;

        var page = services.GetRequiredService<CirclesPage>();
        page.DataContext = services.GetRequiredService<CirclesPageViewModel>();

        if (this.VisualRoot is MainWindow window)
            _ = window.NavRoot.PushAsync(page);
    }

    /// <summary>
    /// DEV ONLY: temporary shortcut to open the signature capture
    /// page from the blog editor. The production entry point is a
    /// SignalR push from Yavsc.Org ("devis received, sign here"),
    /// which is the only path that carries the devis identifier
    /// needed to bind the capture to a specific contract.
    ///
    /// Remove this method and the corresponding button in
    /// MainPage.axaml.cs once the SignalR handler lands.
    /// </summary>
    private void OpenSignatureDev(object? sender, RoutedEventArgs e)
    {
        // Resolve via the App's DI container so the page gets
        // the canonical services (Api client, settings, ...).
        var app = Application.Current as App;
        var services = app?.ServiceProvider;
        if (services is null) return;

        var page = services.GetRequiredService<SignaturePage>();
        page.DataContext = services.GetRequiredService<SignaturePageViewModel>();

        if (this.VisualRoot is MainWindow window)
        {
            _ = window.NavRoot.PushAsync(page);
        }
    }
}
