using Yavsc.Moderation;
using Yavsc.Abstract.Interfaces;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
public class ClaudeModerationService : IModerationService
{
    private readonly AnthropicClient _client;
    private readonly float _autoApproveThreshold;
    private readonly float _autoRejectThreshold;

    public ClaudeModerationService(
        AnthropicClient client,
        IConfiguration config)
    {
        _client = client;
        _autoApproveThreshold = config.GetValue<float>(
            "Moderation:AutoApproveThreshold", 0.7f);
        _autoRejectThreshold = config.GetValue<float>(
            "Moderation:AutoRejectThreshold", 0.9f);
    }

    public async Task<ModerationResult> ModerateAsync(string content, string context)
    {
        var prompt = $$"""
            Tu es un modérateur pour une plateforme de mise en relation prestataires/clients.
            Contexte : {{context}}
            Contenu à modérer : {{content}}

            Réponds UNIQUEMENT en JSON :
            {
              "action": "approved" | "rejected" | "needs_review",
              "reason": "explication courte",
              "confidence": 0.0 à 1.0
            }
            """;

        var response = await _client.Messages.GetClaudeMessageAsync(
            new MessageParameters
            {
                Model = AnthropicModels.Claude45Haiku, // Haiku : rapide et économique
                MaxTokens = 256,
                Messages = [new Message(RoleType.User, prompt)]
            });

        var text = response.Content
            .OfType<TextContent>()
            .FirstOrDefault()?.Text ?? string.Empty;

        return ParseResult(text);
    }

    private ModerationResult ParseResult(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var actionStr = root.GetProperty("action").GetString();
            var reason = root.GetProperty("reason").GetString() ?? string.Empty;
            var confidence = root.GetProperty("confidence").GetSingle();

            var action = actionStr switch
            {
                "approved"     => confidence >= _autoApproveThreshold
                                    ? ModerationAction.Approved
                                    : ModerationAction.NeedsReview,
                "rejected"     => confidence >= _autoRejectThreshold
                                    ? ModerationAction.Rejected
                                    : ModerationAction.NeedsReview,
                "needs_review" => ModerationAction.NeedsReview,
                _              => ModerationAction.NeedsReview
            };

            return new ModerationResult(action, reason, confidence);
        }
        catch
        {
            return new ModerationResult(
                ModerationAction.NeedsReview,
                "Erreur de parsing de la réponse",
                0f);
        }
    }
}