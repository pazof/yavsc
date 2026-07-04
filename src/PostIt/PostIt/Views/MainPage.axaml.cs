using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
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
        var services = app?.Services;
        if (services is null) return;

        var page = services.GetRequiredService<SignaturePage>();
        page.DataContext = services.GetRequiredService<SignaturePageViewModel>();

        if (this.VisualRoot is MainWindow window)
        {
            _ = window.NavRoot.PushAsync(page);
        }
    }
}
