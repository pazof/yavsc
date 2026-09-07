using CommunityToolkit.Mvvm.ComponentModel;
using Yavsc.Models.Haircut;

namespace PostIt.ViewModels;

public partial class SelectableHairPrestationItem : ObservableObject
{
    public long Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public static SelectableHairPrestationItem FromDto(HairPrestationDto dto)
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            Details = dto.Details,
        };
}