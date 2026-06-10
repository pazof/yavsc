using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    // Stocke le ViewModel de la page actuellement affichée
    [ObservableProperty]
    private object? _currentPage;

    // Instances des pages pour éviter de les recréer à chaque fois (Optionnel)
    private readonly HomePageViewModel _homePage = new();
    private readonly SettingsPageViewModel _settingsPage = new();

    public MainWindowViewModel()
    {
        // Définir la page de démarrage par défaut
        CurrentPage = _homePage;
    }

    [RelayCommand]
    private void NavigateToHome() => CurrentPage = _homePage;

    [RelayCommand]
    private void NavigateToSettings() => CurrentPage = _settingsPage;
}
