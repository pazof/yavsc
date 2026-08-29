using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace PostIt.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (DataContext is ViewModels.MainViewModel vm)
        {
            if (!vm.IsLoaded)
            {
                vm.RefreshAsync().Wait();
            }
        }
    }
}
