using Yavsc.Abstract.Workflow;

namespace PostIt.ViewModels;

public sealed class ActivityUserDisplayItem
{
    public string PerformerId { get; init; } = string.Empty;
    public bool HasPerformerProfile { get; init; }
    public string PerformerBadgeLabel { get; init; } = "Profil pro";
    public bool IsPerformerActive { get; init; }
    public string PerformerStatusBadgeLabel { get; init; } = "Inactif";
    public string PerformerStatusBadgeBackground { get; init; } = "#FDECEA";
    public string PerformerStatusBadgeBorder { get; init; } = "#C62828";
    public string PerformerStatusBadgeForeground { get; init; } = "#8E0000";
    public string UserName { get; init; } = string.Empty;
    public string WebSite { get; init; } = string.Empty;
    public int ExtraActivityCount { get; init; }
    public string ExtraActivityLabel { get; init; } = "Pas d'autre activité";

    public static ActivityUserDisplayItem FromDto(ActivityPerformerDto dto)
    {
        return new ActivityUserDisplayItem
        {
            PerformerId = dto.PerformerId,
            HasPerformerProfile = dto.HasPerformerProfile,
            UserName = dto.UserName,
            WebSite = dto.WebSite,
            IsPerformerActive = dto.Active,
            PerformerStatusBadgeLabel = dto.Active ? "Actif" : "Inactif",
            PerformerStatusBadgeBackground = dto.Active ? "#E6F7EC" : "#FDECEA",
            PerformerStatusBadgeBorder = dto.Active ? "#2E7D32" : "#C62828",
            PerformerStatusBadgeForeground = dto.Active ? "#1B5E20" : "#8E0000",
            ExtraActivityCount = dto.ExtraActivityCount,
            ExtraActivityLabel = dto.ExtraActivityCount == 0
                ? "Pas d'autre activité"
                : $"Autres spécialisations: {dto.ExtraActivityCount}"
        };
    }
}
