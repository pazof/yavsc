using System;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Helpers;
using PostIt.Services;
using Yavsc.Api.Client;
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
        OpenProviderRequests = new AsyncRelayCommand(OpenProviderRequestsAsync);
        OpenClientEstimates = new AsyncRelayCommand(() => OpenEstimateListAsync(EstimateListPerspective.Client));
        OpenProviderEstimates = new AsyncRelayCommand(() => OpenEstimateListAsync(EstimateListPerspective.Provider));
        OpenBlogs = new AsyncRelayCommand(App.PushBlogsPageAsync);
        OpenMyFiles = new AsyncRelayCommand(OpenMyFilesAsync);
    }
    public IAsyncRelayCommand OpenBlogs { get; }
    public IAsyncRelayCommand OpenActivities { get; }
    public IAsyncRelayCommand OpenProviderRequests { get; }
    public IAsyncRelayCommand OpenClientEstimates { get; }
    public IAsyncRelayCommand OpenProviderEstimates { get; }
    public IAsyncRelayCommand OpenMyFiles { get; }

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

    private async Task OpenProviderRequestsAsync()
    {
        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var billingClient = app.ServiceProvider?.GetRequiredService<BillingApiClient>();
        if (billingClient is null)
        {
            throw new InvalidOperationException("Client billing indisponible.");
        }

        var estimateClient = app.ServiceProvider?.GetRequiredService<EstimateApiClient>();

        var vm = new ProviderOngoingRequestsPageViewModel(billingClient, Settings, estimateClient);
        await vm.InitializeAsync();
        await app.PushPageAsync(vm);
    }

    private async Task OpenEstimateListAsync(EstimateListPerspective perspective)
    {
        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var estimateClient = app.ServiceProvider?.GetRequiredService<EstimateApiClient>();
        if (estimateClient is null)
        {
            throw new InvalidOperationException("Client devis indisponible.");
        }

        var frontClient = app.ServiceProvider?.GetRequiredService<FrontOfficeApiClient>();
        if (frontClient is null)
        {
            throw new InvalidOperationException("Client front-office indisponible.");
        }

        var vm = new EstimateListPageViewModel(estimateClient, perspective, frontClient);
        await vm.InitializeAsync();
        await app.PushPageAsync(vm);
    }

    /// <summary>
    /// Ouvre la page « Mes fichiers » (espace de stockage personnel),
    /// servie par le host Blogs via <c>api/v1/fs</c>.
    /// </summary>
    private async Task OpenMyFilesAsync()
    {
        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var fsClient = app.ServiceProvider?.GetRequiredService<Yavsc.Api.Client.UserFilesApiClient>();
        if (fsClient is null)
        {
            throw new InvalidOperationException("Client fichiers indisponible.");
        }

        var vm = new MyFilesViewModel(fsClient);
        await vm.InitializeAsync();
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
