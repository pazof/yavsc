using Avalonia.Controls;
using PostIt.Controls;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class EstimateValidationPage : ContentPage
{
    private EstimateValidationPageViewModel? _vm;

    public EstimateValidationPage()
    {
        InitializeComponent();

        DataContextChanged += (_, _) => RebindViewModel(DataContext as EstimateValidationPageViewModel);
        Unloaded += (_, _) => _vm?.Detach();
    }

    private void RebindViewModel(EstimateValidationPageViewModel? vm)
    {
        if (_vm is not null)
        {
            _vm.Detach();
        }

        _vm = vm;

        if (_vm is null || ProPad is null || ClientPad is null) return;

        _vm.Attach(ProPad.SignaturePad, ClientPad.SignaturePad);
    }
}