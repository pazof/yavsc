using System;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Helpers;
using PostIt.Services;
namespace PostIt.ViewModels;

public class HomePageViewModel : ViewModelBase
{
    public YavscApiClient Api { get; }
    public Settings Settings { get; }
    public SessionStatusViewModel SessionStatus { get; }

    private string _welcomeText = "Welcome to PostIt!";
    public string WelcomeText
    {
        get => _welcomeText;
        set => SetProperty(ref _welcomeText, value);
    }

    public override bool CanNavigateNext { get => true; protected set => throw new System.NotImplementedException(); }
    public override bool CanNavigatePrevious { get => false; protected set => throw new System.NotImplementedException(); }

    public HomePageViewModel(YavscApiClient api, Settings settings, SessionStatusViewModel sessionStatus)
    {
        Api = api;
        Settings = settings;
        SessionStatus = sessionStatus;

        OpenActivities = new AsyncRelayCommand(OpenActivitiesAsync);

    }
    public IAsyncRelayCommand OpenBlogs { get; } = new AsyncRelayCommand(App.PushBlogsPageAsync);
    public IAsyncRelayCommand OpenActivities { get; }

    private async Task OpenActivitiesAsync()
    {
        var app = (App?)Application.Current;
        var vm = app?.ServiceProvider?.GetRequiredService<ActivitiesPageViewModel>();
        if (app is null || vm is null)
        {
            throw new InvalidOperationException("Activities page is not available.");
        }

        await vm.RefreshAsync();
        await app.PushPageAsync(vm);
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
    public HomePageViewModel() : this(null!, new Settings(), new SessionStatusViewModel())
    {

    }
}
