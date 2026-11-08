using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PostIt.ViewModels;

/// <summary>
/// ViewModel de la page « À propos de PostIt ». Page statique :
/// version de l'application, description, licence et liens vers le
/// dépôt Forgejo et son suivi de tickets.
/// </summary>
public partial class AboutPageViewModel : ViewModelBase
{
    public const string RepositoryUrl = "https://forgejo.pschneider.fr/notazof/yavsc";
    public const string IssuesUrl = RepositoryUrl + "/issues";

    private readonly Settings? _settings;

    public AboutPageViewModel(Settings? settings = null)
    {
        _settings = settings;
        Version = ReadVersion();
    }

    public string Version { get; }

    public Uri TermsUri => new(new Uri(
        (_settings?.Authentication.Authority ?? AuthenticationSettings.DefaultAuthority).TrimEnd('/') + "/"), "Home/CGU");
    public Uri SubscriptionsUri => new(new Uri(
        (_settings?.Authentication.Authority ?? AuthenticationSettings.DefaultAuthority).TrimEnd('/') + "/"), "ResourceSubscription");

    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    public string Description { get; } =
        "PostIt est le client de la plateforme Yavsc : blog, devis, " +
        "demandes de prestations et fichiers personnels.";

    public string License { get; } =
        "Licence WTFPL — Do What The Fuck You Want To Public License, version 2";

    public override bool CanNavigateNext
    {
        get => false;
        protected set => throw new NotImplementedException();
    }

    public override bool CanNavigatePrevious
    {
        get => true;
        protected set => throw new NotImplementedException();
    }

    [RelayCommand]
    private Task OpenRepository() => OpenUrlAsync(RepositoryUrl);

    [RelayCommand]
    private Task OpenIssues() => OpenUrlAsync(IssuesUrl);

    [RelayCommand]
    private Task OpenTerms() => OpenUrlAsync(TermsUri.AbsoluteUri);

    [RelayCommand]
    private Task OpenSubscriptions() => OpenUrlAsync(SubscriptionsUri.AbsoluteUri);

    private async Task OpenUrlAsync(string url)
    {
        StatusMessage = null;
        var app = (App)App.Current!;
        var topLevel = TopLevel.GetTopLevel(app.View);
        if (topLevel is null || !await topLevel.Launcher.LaunchUriAsync(new Uri(url)).ConfigureAwait(true))
        {
            StatusMessage = "Impossible d'ouvrir le navigateur : " + url;
        }
    }

    private static string ReadVersion()
    {
        var assembly = typeof(AboutPageViewModel).Assembly;
        var info = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        if (!string.IsNullOrEmpty(info))
        {
            // "1.1.0-estimate.1+364.Branch.x.Sha.y" → "1.1.0-estimate.1"
            var plus = info.IndexOf('+');
            return plus > 0 ? info[..plus] : info;
        }

        return assembly.GetName().Version?.ToString() ?? "inconnue";
    }
}
