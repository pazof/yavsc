namespace Yavsc.Workflow
{
    public interface IPerformerProfile
    {
        string PerformerId { get; set; }
        string SIREN { get; set; }
        bool AcceptNotifications { get; set; }
        long OrganizationAddressId { get; set; }
        bool AcceptPublicContact { get; set; }
        bool UseGeoLocalizationToReduceDistanceWithClients { get; set; }
        string WebSite { get; set; }
        bool Active { get; set; }
        int? MaxDailyCost { get; set; }
        int? MinDailyCost { get; set; }
        int Rate { get; set; }
    }
}
