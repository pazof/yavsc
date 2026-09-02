namespace Yavsc.Abstract.Workflow;

/// <summary>
/// Lightweight command-form description exposed to API clients.
/// </summary>
public sealed class CommandFormSummary
{
    public long Id { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
