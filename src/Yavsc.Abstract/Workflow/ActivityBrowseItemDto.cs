namespace Yavsc.Abstract.Workflow;

/// <summary>
/// Activity node returned by the browsing API.
/// </summary>
public sealed class ActivityBrowseItemDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ParentCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Photo { get; set; } = string.Empty;
    public int Rate { get; set; }
    public int PerformerCount { get; set; }
    public List<CommandFormSummaryDto> Forms { get; set; } = new();
    public List<ActivityBrowseItemDto> Children { get; set; } = new();
}
