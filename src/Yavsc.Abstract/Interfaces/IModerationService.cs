using Yavsc.Moderation;

namespace Yavsc.Abstract.Interfaces;

public interface IModerationService
{
    Task<ModerationResult> ModerateAsync(string content, string context);
}
