using Avalonia.Controls;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (!Design.IsDesignMode && DataContext is HomePageViewModel vm
            && vm.RefreshNotificationsCommand.CanExecute(null))
            _ = vm.RefreshNotificationsAsync();
    }
}
