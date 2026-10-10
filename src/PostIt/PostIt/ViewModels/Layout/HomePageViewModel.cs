using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;
using PostIt.ViewModels.Chat;
using Yavsc.Abstract.Models.Messaging;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public class HomePageViewModel : ViewModelBase
{
    public YavscApiClient Api { get; }
    public Settings Settings { get; }
    public SessionStatusViewModel SessionStatus { get; }
    public ObservableCollection<Notification> Notifications { get; } = new();
    private readonly NotificationsApiClient? _notificationsClient;
    private int _notificationsLoadVersion;
    private bool _isLoadingNotifications;
    private string? _notificationsError;
    private bool _notificationsLoaded;

    public bool IsLoadingNotifications
    {
        get => _isLoadingNotifications;
        private set => SetProperty(ref _isLoadingNotifications, value);
    }

    public string? NotificationsError
    {
        get => _notificationsError;
        private set => SetProperty(ref _notificationsError, value);
    }

    public bool HasNoNotifications => _notificationsLoaded && Notifications.Count == 0;

    private string _welcomeText = "Welcome to PostIt!";
    public string WelcomeText
    {
        get => _welcomeText;
        set => SetProperty(ref _welcomeText, value);
    }

    public override bool CanNavigateNext { get => true; protected set => throw new System.NotImplementedException(); }
    public override bool CanNavigatePrevious { get => false; protected set => throw new System.NotImplementedException(); }

    public HomePageViewModel(YavscApiClient api, Settings settings, SessionStatusViewModel sessionStatus,
        NotificationsApiClient notificationsClient)
    {
        Api = api;
        Settings = settings;
        SessionStatus = sessionStatus;
        _notificationsClient = notificationsClient;
        SessionStatus.PropertyChanged += OnSessionStatusChanged;
        RefreshNotificationsCommand = new AsyncRelayCommand(RefreshNotificationsAsync,
            () => _notificationsClient is not null);

        OpenActivities = new AsyncRelayCommand(OpenActivitiesAsync);
        OpenProviderRequests = new AsyncRelayCommand(OpenProviderRequestsAsync);
        OpenClientEstimates = new AsyncRelayCommand(() => OpenEstimateListAsync(EstimateListPerspective.Client));
        OpenProviderEstimates = new AsyncRelayCommand(() => OpenEstimateListAsync(EstimateListPerspective.Provider));
        OpenBlogs = new AsyncRelayCommand(App.PushBlogsPageAsync);
        OpenMyFiles = new AsyncRelayCommand(OpenMyFilesAsync);
        OpenProfile = new AsyncRelayCommand(OpenProfileAsync);
        OpenPerformerConfiguration = new AsyncRelayCommand(OpenPerformerConfigurationAsync);
        OpenAdministration = new AsyncRelayCommand(OpenAdministrationAsync);
        OpenResourceUsage = new AsyncRelayCommand(OpenResourceUsageAsync);
        OpenChat = new AsyncRelayCommand(OpenChatAsync);
    }


    public IAsyncRelayCommand OpenChat { get; }
    public IAsyncRelayCommand OpenBlogs { get; }
    public IAsyncRelayCommand OpenProfile { get; }
    public IAsyncRelayCommand OpenPerformerConfiguration { get; }
    public IAsyncRelayCommand OpenAdministration { get; }
    public IAsyncRelayCommand OpenResourceUsage { get; }
    public IAsyncRelayCommand OpenActivities { get; }
    public IAsyncRelayCommand OpenProviderRequests { get; }
    public IAsyncRelayCommand OpenClientEstimates { get; }
    public IAsyncRelayCommand OpenProviderEstimates { get; }
    public IAsyncRelayCommand OpenMyFiles { get; }
    public IAsyncRelayCommand RefreshNotificationsCommand { get; }

    public async Task RefreshNotificationsAsync()
    {
        if (_notificationsClient is null)
            throw new InvalidOperationException("Notifications client is not available.");

        var version = ++_notificationsLoadVersion;
        Notifications.Clear();
        NotificationsError = null;
        _notificationsLoaded = false;
        OnPropertyChanged(nameof(HasNoNotifications));
        IsLoadingNotifications = true;
        try
        {
            var notifications = SessionStatus.IsLoggedIn
                ? await _notificationsClient.GetCurrentAsync()
                : await _notificationsClient.GetPublicAsync();
            if (version != _notificationsLoadVersion)
                return;
            if (notifications is null)
                throw new InvalidOperationException("The notifications response is empty.");

            foreach (var notification in notifications)
                Notifications.Add(notification);
            _notificationsLoaded = true;
        }
        catch (Exception ex)
        {
            if (version == _notificationsLoadVersion)
                NotificationsError = $"Impossible de charger les notifications : {ex.Message}";
        }
        finally
        {
            if (version == _notificationsLoadVersion)
            {
                IsLoadingNotifications = false;
                OnPropertyChanged(nameof(HasNoNotifications));
            }
        }
    }

    private void OnSessionStatusChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SessionStatusViewModel.IsLoggedIn) || _notificationsClient is null)
            return;

        // Logout can complete on a worker thread; collections and bindings belong to the UI thread.
        if (Dispatcher.UIThread.CheckAccess())
            _ = RefreshNotificationsAsync();
        else
            Dispatcher.UIThread.Post(() => _ = RefreshNotificationsAsync());
    }

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

    private async Task OpenChatAsync()
    {
        var app = (App?)Application.Current;
        var vm = app!.ServiceProvider?.GetRequiredService<ChatViewModel>();
        await app.PushPageAsync(vm!);

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

    private async Task OpenProfileAsync()
    {
        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var vm = app.ServiceProvider?.GetRequiredService<UserProfilePageViewModel>();
        if (vm is null)
        {
            throw new InvalidOperationException("Page profil indisponible.");
        }

        await vm.InitializeAsync();
        await app.PushPageAsync(vm);
    }

    private async Task OpenPerformerConfigurationAsync()
    {
        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var vm = app.ServiceProvider?.GetRequiredService<PerformerConfigurationPageViewModel>();
        if (vm is null)
        {
            throw new InvalidOperationException("Page performer indisponible.");
        }

        await vm.InitializeAsync();
        await app.PushPageAsync(vm);
    }

    private async Task OpenAdministrationAsync()
    {
        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var vm = app.ServiceProvider?.GetRequiredService<AdministrationPageViewModel>();
        if (vm is null)
        {
            throw new InvalidOperationException("Page administration indisponible.");
        }

        await vm.InitializeAsync();
        await app.PushPageAsync(vm);
    }

    private async Task OpenResourceUsageAsync()
    {
        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var resourceUsageClient = app.ServiceProvider?.GetRequiredService<ResourceUsageApiClient>();
        if (resourceUsageClient is null)
        {
            throw new InvalidOperationException("Client usage indisponible.");
        }

        var vm = new ResourceUsagePageViewModel(resourceUsageClient);
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
    public HomePageViewModel() : this(null!, new Settings(), new SessionStatusViewModel(), null!)
    {

    }
}
