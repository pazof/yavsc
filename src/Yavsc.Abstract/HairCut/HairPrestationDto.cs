namespace Yavsc.Models.Haircut;

/// <summary>
/// Lightweight hair-prestation description exposed to API clients.
/// </summary>
public sealed class HairPrestationDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}