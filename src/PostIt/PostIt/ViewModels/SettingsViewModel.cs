using CommunityToolkit.Mvvm.ComponentModel;

namespace PostIt.ViewModels;

public partial class SettingsPageViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial bool DarkMode { get; set; }

    [ObservableProperty]
    public partial string Authority { get; set; }

    [ObservableProperty]
    public partial string ClientId { get; set; }

    public override bool CanNavigateNext { get => false; protected set => throw new System.NotImplementedException(); }
    public override bool CanNavigatePrevious { get => true; protected set => throw new System.NotImplementedException(); }
}
