using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class EstimateListPage : ContentPage
{
    public EstimateListPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Re-fetch the list when returning to it (e.g. after validating an
    // estimate on the validation page). Avalonia's Page.OnNavigatedTo
    // fires on the page popped back to with NavigationType.Pop; the list
    // VM is retained on the nav stack while away, so this refresh picks
    // up the server-side filter change that removed the signed estimate.
    protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.NavigationType == NavigationType.Pop
            && DataContext is EstimateListPageViewModel vm)
        {
            _ = vm.RefreshAsync();
        }
    }
}
