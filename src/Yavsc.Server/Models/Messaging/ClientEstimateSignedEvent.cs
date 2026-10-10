using Yavsc.Interfaces.Workflow;
using Yavsc.Models.Billing;

namespace Yavsc.Models.Messaging;

public sealed class ClientEstimateSignedEvent : IEvent
{
    public ClientEstimateSignedEvent(Estimate estimate, string sender)
    {
        EstimateId = estimate.Id;
        EstimateTitle = estimate.Title;
        PerformerId = estimate.OwnerId;
        Sender = sender;
    }

    public long EstimateId { get; }
    public string EstimateTitle { get; }
    public string PerformerId { get; }
    public string Topic => "ClientEstimateSigned";
    public string Sender { get; set; }
    public string Title => "Devis signé par le client";
    public string CreateBody() => $"{Sender} a signé le devis n°{EstimateId} : {EstimateTitle}.";
}
