using CommunityToolkit.Mvvm.ComponentModel;

namespace PostIt.ViewModels;

/// <summary>
/// Editable estimate line. <see cref="Count"/> is exposed as a
/// <see cref="decimal"/> so it binds directly to
/// <c>NumericUpDown.Value</c> (<c>decimal?</c>); it is rounded back
/// to an integer when the DTO is built.
/// </summary>
public partial class EstimateLineItemViewModel : ObservableObject
{
    public long Id { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(LineTotal))]
    [NotifyPropertyChangedFor(nameof(LineTotalLabel))]
    public partial decimal Count { get; set; } = 1m;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(LineTotal))]
    [NotifyPropertyChangedFor(nameof(LineTotalLabel))]
    public partial decimal UnitaryCost { get; set; }

    [ObservableProperty]
    public partial string Currency { get; set; } = "EUR";

    public decimal LineTotal => Count * UnitaryCost;

    public string LineTotalLabel => $"{LineTotal:0.00}";
}
