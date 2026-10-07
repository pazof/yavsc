#nullable enable

namespace Yavsc.Abstract.Identity;

public class ProfileUpdateRequest
{
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? GoogleCalendarId { get; set; }
    public string? BankInfoSummary { get; set; }
}
