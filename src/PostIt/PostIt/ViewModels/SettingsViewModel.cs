using CommunityToolkit.Mvvm.ComponentModel;

namespace PostIt.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial bool DarkMode { get; set; }

    [ObservableProperty]
    public partial string Authority { get; set; }

    [ObservableProperty]
    public partial string ClientId { get; set; }

    [ObservableProperty]
    public partial string ClientSecret { get; set; }

}
