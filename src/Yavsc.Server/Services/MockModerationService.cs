using Yavsc.Abstract.Interfaces;
using Yavsc.Moderation;

public class MockModerationService : IModerationService
{
    public Task<ModerationResult> ModerateAsync(string content, string context)
        => Task.FromResult(new ModerationResult(
            ModerationAction.Approved,
            "Mock modération — API non configurée",
            1.0f));
}