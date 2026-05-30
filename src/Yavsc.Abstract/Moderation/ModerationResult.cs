namespace Yavsc.Moderation;

public record ModerationResult(
    ModerationAction Action,
    string Reason,
    float ConfidenceScore
);
