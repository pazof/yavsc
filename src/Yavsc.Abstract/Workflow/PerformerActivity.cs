namespace Yavsc.Abstract.Workflow;

/// <summary>
/// Lightweight performer description returned for one activity.
/// </summary>
public sealed class PerformerActivity
{
    public string PerformerId { get; set; } = string.Empty;
    public bool HasPerformerProfile { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool Active { get; set; }
    public bool AcceptNotifications { get; set; }
    public bool AcceptPublicContact { get; set; }
    public string WebSite { get; set; } = string.Empty;
    public string ActivityCode { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string SettingsClassName { get; set; } = string.Empty;
    public int ExtraActivityCount { get; set; }
}
