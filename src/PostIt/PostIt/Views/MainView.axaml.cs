using System;
using Avalonia.Controls;

namespace PostIt.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is ViewModels.MainViewModel vm)
        {
            if (!vm.IsLoaded)
            {
                vm.RefreshAsync().Wait();
            }
        }
    }
}
