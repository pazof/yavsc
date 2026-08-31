using System;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public sealed class BillingQueryDisplayItem
{
    public long Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string StatusLabel { get; init; } = string.Empty;
    public string EventDateLabel { get; init; } = string.Empty;
    public string BillingCode { get; init; } = string.Empty;

    public static BillingQueryDisplayItem FromDto(BillingQuerySummaryDto dto)
    {
        var summary = !string.IsNullOrWhiteSpace(dto.Reason)
            ? dto.Reason
            : !string.IsNullOrWhiteSpace(dto.AdditionalInfo)
                ? dto.AdditionalInfo
                : dto.Description;

        return new BillingQueryDisplayItem
        {
            Id = dto.Id,
            Description = string.IsNullOrWhiteSpace(dto.Description)
                ? $"Commande #{dto.Id}"
                : dto.Description,
            Summary = summary,
            StatusLabel = dto.Status.ToString(),
            EventDateLabel = dto.EventDate?.ToLocalTime().ToString("g") ?? "Date non précisée",
            BillingCode = dto.BillingCode,
        };
    }
}