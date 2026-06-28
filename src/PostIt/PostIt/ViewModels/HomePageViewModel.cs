using PostIt;
using PostIt.Services;
using PostIt.ViewModels;

public class HomePageViewModel : ViewModelBase
{
    public YavscApiClient Api { get; }
    public Settings Settings { get; }

    private string _welcomeText = "Welcome to PostIt!";
    public string WelcomeText
    {
        get => _welcomeText;
        set => SetProperty(ref _welcomeText, value);
    }

    public override bool CanNavigateNext { get => true; protected set => throw new System.NotImplementedException(); }
    public override bool CanNavigatePrevious { get => false; protected set => throw new System.NotImplementedException(); }

    public HomePageViewModel(YavscApiClient api, Settings settings)
    {
        Api = api;
        Settings = settings;
    }

    /// <summary>
    /// Avalonia designer constructor. Builds a self-contained VM
    /// with a freshly-constructed Settings so the XAML preview can
    /// render without a running App. Production paths always reach
    /// the parameterised constructor (DI or direct injection), and
    /// the postit://callback crash is fixed at the Settings layer
    /// (thread-safe dispatcher marshalling on PropertyChanged) — a
    /// designer-only duplicate instance is therefore harmless.
    /// </summary>
    public HomePageViewModel() : this(null!, new Settings()) { }
}
