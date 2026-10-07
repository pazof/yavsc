namespace Yavsc.Abstract.Workflow;

public sealed class PerformerProfileSettings
{
    public string PerformerId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ExerciseCountryCode { get; set; } = "fr";
    public string SIREN { get; set; } = string.Empty;
    public string WebSite { get; set; } = string.Empty;
    public bool Active { get; set; }
    public bool AcceptNotifications { get; set; }
    public bool AcceptPublicContact { get; set; }
    public bool UseGeoLocalizationToReduceDistanceWithClients { get; set; }
    public List<string> SelectedActivityCodes { get; set; } = new();
}
